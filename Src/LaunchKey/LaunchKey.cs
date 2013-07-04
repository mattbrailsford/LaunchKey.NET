using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LaunchKey.Extensions;
using LaunchKey.Models;
using RestSharp;
using RestSharp.Deserializers;

namespace LaunchKey
{
    /// <summary>
    /// Class for accessing the LaunchKey API
    /// </summary>
    public class LaunchKey
    {
        private const string ApiHostFormat = "https://api.launchkey.com/{0}/";

        private readonly RestClient _client;

        private string _apiPublicKey;
        private string _appKey;
        private string _appSecret;
        private string _privateKey;
        private string _domain;

        private DateTime _pingTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchKey" /> class.
        /// </summary>
        /// <param name="appKey">The app key.</param>
        /// <param name="appSecret">The app secret.</param>
        /// <param name="privateKey">The private key.</param>
        /// <param name="domain">The domain.</param>
        /// <param name="version">The version.</param>
        /// <param name="debug">if set to <c>true</c> enable debugging.</param>
        public LaunchKey(string appKey, string appSecret, string privateKey, 
            string domain, string version = "v1", bool debug = false)
        {
            _appKey = appKey;
            _appSecret = appSecret;
            _privateKey = privateKey;
            _domain = domain;

            _client = new RestClient(string.Format(ApiHostFormat, version));

            if (debug)
                _client.Proxy = HttpWebRequest.GetSystemWebProxy();
        }

        /// <summary>
        /// Pings LaunchKey.
        /// </summary>
        /// <returns></returns>
        public PingResponse Ping()
        {
            var request = new RestRequest("ping", Method.GET);
            var response = _client.Execute<PingResponse>(request).Data;

            _apiPublicKey = response.Key;
            _pingTime = response.LaunchkeyTime;

            return response;
        }

        /// <summary>
        /// Sends an authentication request to LaunchKey for the specified username.
        /// </summary>
        /// <param name="username">The username.</param>
        public AuthorizeResponse Authorize(string username) 
        {
            var parameters = PrepareAuthParameters();
            parameters.Add("username", username);

            var request = new RestRequest("auths", Method.POST);
            request.AddParameters(parameters);

            return _client.Execute<AuthorizeResponse>(request).Data;
        }

        /// <summary>
        /// Polls LaunchKey to check whether the authentication has been completed.
        /// </summary>
        /// <param name="authRequest">The auth request.</param>
        /// <returns></returns>
        public PollResponse Poll(string authRequest)
        {
            var parameters = PrepareAuthParameters();
            parameters.Add("auth_request", authRequest);

            var request = new RestRequest("poll", Method.GET);
            request.AddParameters(parameters);

            return _client.Execute<PollResponse>(request).Data;
        }

        /// <summary>
        /// Determines whether the response from the poll.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns>
        ///   <c>true</c> if the specified package is authorized; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAuthorized(string package)
        {
            var decryptedPackage = RSADecrypt(_privateKey, package);
            var authResponse = new JsonDeserializer()
                .Deserialize<UserAuthorizeResponse>(decryptedPackage);

            if(authResponse.Response)
            {
                Notify(NotifyAction.Authenticate, true, authResponse.AuthRequest);
                return true;
            }

            Notify(NotifyAction.Authenticate, false);
            return false;
        }

        /// <summary>
        /// Send a notification for the specified action to LaunchKey.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="status">The status.</param>
        /// <param name="authRequest">The auth request.</param>
        /// <param name="username">The username.</param>
        public void Notify(NotifyAction action, bool status, string authRequest = "", string username = "")
        {
            var parameters = PrepareAuthParameters();
            parameters.Add("action", action.ToString());
            parameters.Add("status", status.ToString());
            parameters.Add("auth_request", authRequest);

            if(!string.IsNullOrEmpty(username))
                parameters.Add("username", username);

            var request = new RestRequest("logs", Method.PUT);
            request.AddParameters(parameters);

            _client.Execute(request);
        }

        /// <summary>
        /// Handles the deorbit notification from LaunchKey and returns the user hash needed to identify the user to logout.
        /// </summary>
        /// <param name="orbit">The orbit.</param>
        /// <param name="signature">The signature.</param>
        /// <returns></returns>
        public string HandleDeorbit(string orbit, string signature)
        {
            Ping();

            if(RSAVerifySign(_apiPublicKey, signature, orbit))
            {
                var decodedOrbit = new JsonDeserializer()
                    .Deserialize<Orbit>(orbit);
                if((_pingTime - decodedOrbit.LaunckeyTime).TotalMinutes > 5)
                {
                    return decodedOrbit.UserHash;
                }
            }

            return null;
        }

        /// <summary>
        /// Prepares the default auth parameters.
        /// </summary>
        /// <returns></returns>
        private IDictionary<string, string> PrepareAuthParameters()
        {
            if(string.IsNullOrEmpty(_apiPublicKey))
                Ping();

            var toEncrypt = string.Format("{{\"secret\" : \"{0}\", \"stamped\" : \"{1}\"}}", 
                _appSecret,
                _pingTime.ToString("yyyy-MM-yy hh:mm:ss"));

            var encryptedAppSecret = RSAEncrypt(_apiPublicKey, toEncrypt);
            var signature = RSASign(_privateKey, encryptedAppSecret);
            var parameters = new Dictionary<string, string>
            {
                { "app_key", _appKey },
                { "secret_key", encryptedAppSecret },
                { "signature", signature }
            };

            return parameters;
        }

        /// <summary>
        /// RSA encrypts the provided string using the supplied public key.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="toEncrypt">To encrypt.</param>
        /// <returns></returns>
        private string RSAEncrypt(string publicKey, string toEncrypt)
        {
            string encryptedValue;

            using(var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.LoadPublicKeyPEM(publicKey);

                var encoder = new ASCIIEncoding();
                var dataToEncrypt = encoder.GetBytes(toEncrypt);
                var encryptedData = rsa.Encrypt(dataToEncrypt, true);
                encryptedValue = Convert.ToBase64String(encryptedData);
            }

            return encryptedValue;
        }

        /// <summary>
        /// RSA decrypts the provided string using the supplied private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        private string RSADecrypt(string privateKey, string package)
        {
            package = package.StripNoneBase64Chars();

            string decryptedValue;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.LoadPrivateKeyPEM(privateKey);

                var encoder = new ASCIIEncoding();
                var dataToDecrypt = Convert.FromBase64String(package);
                var decryptedData = rsa.Decrypt(dataToDecrypt, true);
                decryptedValue = encoder.GetString(decryptedData);
            }

            return decryptedValue;
        }

        /// <summary>
        /// RSA signs the passed in string using the supplied private key.
        /// </summary>
        /// <param name="privateKey">The private key.</param>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        private string RSASign(string privateKey, string package)
        {
            string signature;

            using(var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.LoadPrivateKeyPEM(privateKey);

                var signedData = rsa.SignData(Convert.FromBase64String(package), new SHA256CryptoServiceProvider());
                signature = Convert.ToBase64String(signedData);
            }

            return signature;
        }

        /// <summary>
        /// RSAs the verify sign.
        /// </summary>
        /// <param name="publicKey">The public key.</param>
        /// <param name="signature">The signature.</param>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        private bool RSAVerifySign(string publicKey, string signature, string package)
        {
            package = package.StripNoneBase64Chars();

            var result = false;

            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.PersistKeyInCsp = false;
                rsa.LoadPublicKeyPEM(publicKey);

                var encoder = new ASCIIEncoding();
                result = rsa.VerifyData(Convert.FromBase64String(package),
                    new SHA256CryptoServiceProvider(),
                    encoder.GetBytes(signature));
            }

            return result;
        }
    }
}
