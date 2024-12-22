namespace MTCG.Models
{
    public class HttpHeader
    {
        public required string Method { get; set; }
        public required string Path { get; set; }
        public required string Version { get; set; }
        public Dictionary<string, string> Headers { get; set; } // Dictionary to store all other header fields

        public HttpHeader()
        {
            // Initialize empty dictionary
            Headers = new Dictionary<string, string>();
        }
    }
}
