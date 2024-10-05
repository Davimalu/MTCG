using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class HTTPHeader
    {
        public required string Method { get; set; }
        public required string Path { get; set; }
        public required string Version { get; set; }

        // Dictionary to store all other header fields
        public Dictionary<string, string> Headers { get; set; }

        public HTTPHeader()
        {
            // Initialize empty dictionary
            Headers = new Dictionary<string, string>();
        }

    }
}
