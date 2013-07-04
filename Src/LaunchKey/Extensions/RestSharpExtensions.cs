using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Deserializers;

namespace LaunchKey.Extensions
{
    internal static class RestSharpExtensions
    {
        public static void AddParameters(this RestRequest request, IDictionary<string, string> parameters)
        {
            foreach (var parameter in parameters)
                request.AddParameter(parameter.Key, parameter.Value);
        }

        public static TEntity Deserialize<TEntity>(this JsonDeserializer serializer, string toDeserialize)
        {
            return serializer.Deserialize<TEntity>(new RestResponse { Content = toDeserialize });
        }
    }
}
