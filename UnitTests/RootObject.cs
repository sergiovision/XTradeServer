using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    /// <summary>
    /// For Json Deserialisation.
    /// </summary>
    public class RootObject
    {

        /// <summary>
        /// The Web Api Access Token. Gets added to the Header in each communication.
        /// </summary>
        public string access_token { get; set; }



        /// <summary>
        /// The Token Type
        /// </summary>
        public string token_type { get; set; }



        /// <summary>
        /// Expiry.
        /// </summary>
        public int expires_in { get; set; }



        /// <summary>
        /// The Username.
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// Issued.
        /// </summary>
        public string issued { get; set; }

        /// <summary>
        /// Expiry.
        /// </summary>
        public string expires { get; set; }
    }

}
