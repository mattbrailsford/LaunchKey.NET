using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchKey.Models
{
    /// <summary>
    /// Represents a deserialized user auth response
    /// </summary>
    internal class UserAuthorizeResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="UserAuthorizeResponse"/> is successfull.
        /// </summary>
        /// <value>
        ///   <c>true</c> if successfull; otherwise, <c>false</c>.
        /// </value>
        public bool Response { get; set; }

        /// <summary>
        /// Gets or sets the app pins.
        /// </summary>
        /// <value>
        /// The app pins.
        /// </value>
        public string AppPins { get; set; }

        /// <summary>
        /// Gets or sets the auth request.
        /// </summary>
        /// <value>
        /// The auth request.
        /// </value>
        public string AuthRequest { get; set; }

        /// <summary>
        /// Gets or sets the device id.
        /// </summary>
        /// <value>
        /// The device id.
        /// </value>
        public int DeviceId { get; set; }
    }
}
