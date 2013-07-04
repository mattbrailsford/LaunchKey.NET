using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchKey.Models
{
    /// <summary>
    /// Represents the return value of a LaunchKey poll request
    /// </summary>
    public class PollResponse
    {
        /// <summary>
        /// Gets or sets the auth.
        /// </summary>
        /// <value>
        /// The auth.
        /// </value>
        public string Auth { get; set; }

        /// <summary>
        /// Gets or sets the user hash.
        /// </summary>
        /// <value>
        /// The user hash.
        /// </value>
        public string UserHash { get; set; }
    }
}
