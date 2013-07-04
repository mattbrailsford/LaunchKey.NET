using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LaunchKey.Models
{
    /// <summary>
    /// Represents a deserialized orbit entity
    /// </summary>
    internal class Orbit
    {
        /// <summary>
        /// Gets or sets the user hash.
        /// </summary>
        /// <value>
        /// The user hash.
        /// </value>
        public string UserHash { get; set; }

        /// <summary>
        /// Gets or sets the launckey time.
        /// </summary>
        /// <value>
        /// The launckey time.
        /// </value>
        public DateTime LaunckeyTime { get; set; }
    }
}
