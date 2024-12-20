using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCG.HTTP
{
    public class HTTPService
    {
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

        public HTTPHeader ParseHTTPHeader(StreamReader reader)
        {
            string? line;
            line = reader.ReadLine();

            // Check if HTTP header is present
            if (line == null)
            {
                throw new Exception("No HTTP header found");
            }

            var httpParts = line.Split(' ');

            var method = httpParts[0];
            var path = httpParts[1];
            var version = httpParts[2];

            HTTPHeader headers = new HTTPHeader()
            {
                Method = method,
                Path = path,
                Version = version
            };

            while ((line = reader.ReadLine()) != null)
            {
                // Empty line indicates end of HTTP headers
                if (line.Length == 0)
                {
                    break;
                }

                // Parse header fields
                var headerParts = line.Split(":");

                var headerName = headerParts[0].Trim();
                var headerValue = headerParts[1].Trim();

                headers.Headers.Add(headerName, headerValue);
            }

            return headers;
        }

        public string? ParseHTTPBody(StreamReader reader, HTTPHeader headers)
        {
            if (headers.Headers.ContainsKey("Content-Length"))
            {
                int contentLength = int.Parse(headers.Headers["Content-Length"]);

                char[] buffer = new char[contentLength];
                reader.Read(buffer, 0, contentLength);

                string body = new string(buffer);

                return body;
            }
            else
            {
                // No body to parse
                return null;
            }
        }

        private bool IsJson(string? source)
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
