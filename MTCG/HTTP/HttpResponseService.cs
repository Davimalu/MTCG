using System.Text.Json;

namespace MTCG.HTTP
{
    public class HttpResponseService : IHttpResponseService
    {
        /// <summary>
        /// sends a well-formatted HTTP Response to the client
        /// </summary>
        /// <param name="writer">StreamWriter object associated with the TcpClient to which the response is to be sent</param>
        /// <param name="statusCode">Http Status Code</param>
        /// <param name="response">Http Response body</param>
        public void SendResponseToClient(StreamWriter writer, int statusCode, string? response)
        {
            string reasonPhrase;

            switch (statusCode)
            {
                case 200:
                    reasonPhrase = "OK";
                    break;
                case 201:
                    reasonPhrase = "Created";
                    break;
                case 400:
                    reasonPhrase = "Bad Request";
                    break;
                case 401:
                    reasonPhrase = "Unauthorized";
                    break;
                case 402:
                    reasonPhrase = "Payment Required";
                    break;
                case 403:
                    reasonPhrase = "Forbidden";
                    break;
                case 404:
                    reasonPhrase = "Not Found";
                    break;
                case 405:
                    reasonPhrase = "Method Not Allowed";
                    break;
                case 409:
                    reasonPhrase = "Conflict";
                    break;
                case 410:
                    reasonPhrase = "Gone";
                    break;
                case 500:
                    reasonPhrase = "Internal Server Error";
                    break;
                default:
                    reasonPhrase = "Unknown";
                    break;
            }

            writer.WriteLine($"HTTP/1.1 {statusCode} {reasonPhrase}");

            // Check if response is JSON
            if (IsJson(response))
            {
                writer.WriteLine("Content-Type: application/json");
            }
            else
            {
                writer.WriteLine("Content-Type: text/plain");
            }

            writer.WriteLine($"Content-Length: {(response?.Length ?? 0)}");
            writer.WriteLine();

            // HTTP Response Body
            writer.WriteLine(response);
        }


        public bool IsJson(string? source)
        {
            // https://stackoverflow.com/questions/58629279/validate-if-string-is-valid-json-fastest-way-possible-in-net-core-3-0
            if (source == null)
                return false;

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(source))
                {
                    // dispose any created doc
                }
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
