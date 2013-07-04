using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchKey.Models
{
    /// <summary>
    /// Represents the return value of a LaunchKey ping request
    /// </summary>
    public class PingResponse
    {
        /// <summary>
        /// Gets or sets the date stamp.
        /// </summary>
        /// <value>
        /// The date_stamp.
        /// </value>
        public DateTime DateStamp { get; set; }

        /// <summary>
        /// Gets or sets the launchkey time.
        /// </summary>
        /// <value>
        /// The launchkey_time.
        /// </value>
        public DateTime LaunchkeyTime { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }
    }
}
