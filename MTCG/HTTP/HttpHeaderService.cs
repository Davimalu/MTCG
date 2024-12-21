using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using System.Text.RegularExpressions;

namespace MTCG.HTTP
{
    public class HttpHeaderService : IHttpHeaderService
    {
        private readonly IEventService _eventService = new EventService();


        public HTTPHeader? ParseHttpHeader(StreamReader reader)
        {
            // First line of HTTP Header, should be something like    GET /users HTTP/1.1
            string? line = reader.ReadLine();

            // Check if HTTP header is present
            if (line == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Cannot process request: No HTTP Header found", null);
                return null;
            }

            if (!RequestLineIsValid(line))
            {
                return null;
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

                // Skip invalid lines
                if (!HeaderLineIsValid(line))
                {
                    continue;
                }

                // Parse header fields
                var headerParts = line.Split(":");

                var headerName = headerParts[0].Trim();
                var headerValue = headerParts[1].Trim();

                headers.Headers.Add(headerName, headerValue);
            }

            return headers;
        }


        public string? GetTokenFromHeader(HTTPHeader headers)
        {
            // Provided authorization string should be something like "Bearer admin-mtcgToken"

            // Check if authorization header was provided
            if (!headers.Headers.ContainsKey("Authorization") ||
                !IsValidAuthorizationField(headers.Headers["Authorization"]))
            {
                return null;
            }

            // Return token
            return headers.Headers["Authorization"].Split(' ')[1];
        }


        public bool IsValidAuthorizationField(string token)
        {
            // Use @ to ignore escape sequences in string: https://stackoverflow.com/questions/556133/whats-the-in-front-of-a-string-in-c
            return Regex.IsMatch(token, @"^Bearer\s+\w+-mtcgToken$"); // https://regex101.com/r/iTbKSU/1
        }


        public Dictionary<string, string> GetQueryParameters(HTTPHeader headers)
        {
            Dictionary<string, string> queryParameters = new Dictionary<string, string>(); // Dictionary to store key Value Pairs

            // the path will contain something like "/deck?format=plain&test=true"

            if (headers.Path.Contains('?'))
            {
                string queryString = headers.Path.Split('?')[1]; // format=plain&test=true

                if (!string.IsNullOrWhiteSpace(queryString))
                {
                    string[] keyValuePairs = queryString.Split('&'); // format=plain    test=true

                    foreach (string keyValuePair in keyValuePairs)
                    {
                        string[] keyValue = keyValuePair.Split('=');
                        string key = keyValue[0];   // format
                        string value = keyValue.Length > 1 ? keyValue[1] : string.Empty;    // plain
                        queryParameters[key] = value; // queryParameters[format] = plain
                    }
                }
            }

            return queryParameters;
        }


        public string GetPathWithoutQueryParameters(HTTPHeader headers)
        {
            if (headers.Path.Contains('?'))
            {
                return headers.Path.Split('?')[0];
            }

            return headers.Path;
        }


        /// <summary>
        /// Checks if the provided string contains a valid HTTP Request line, e.g. GET /users HTTP/1.1
        /// </summary>
        /// <param name="requestLine">the string to check</param>
        /// <returns></returns>
        private bool RequestLineIsValid(string requestLine)
        {
            // Get Header Parts
            var httpParts = requestLine.Split(' ');

            if (httpParts.Length != 3)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't process request: Malformed HTTP Header", null);
                return false;
            }

            var method = httpParts[0];
            var path = httpParts[1];
            var version = httpParts[2];

            // Check for valid Method
            string[] validMethods = ["GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS", "CONNECT", "TRACE", "PATCH"];
            if (!validMethods.Contains(method))
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't process request: Invalid HTTP Method", null);
                return false;
            }

            // Check for valid path (must start with /)
            if (!path.StartsWith("/"))
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't process request: Invalid Path", null);
                return false;
            }

            // Validate HTTP version
            if (!Regex.IsMatch(version, @"^HTTP/\d+\.\d+$"))
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't process request: Invalid HTTP Version", null);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Checks if the provided string contains a valid HTTP Header line, e.g. Content-Type: application/json
        /// </summary>
        /// <param name="headerLine">the string to check</param>
        /// <returns></returns>
        private bool HeaderLineIsValid(string headerLine)
        {
            // Check for key:value pairs
            int colonIndex = headerLine.IndexOf(':');

            // Check if colon exists (must not be the first character)
            if (colonIndex <= 0)
            {
                _eventService.LogEvent(EventType.Warning, $"{headerLine}", null);
                _eventService.LogEvent(EventType.Warning, $"Couldn't process Header line: Malformed HTTP Header", null);
                return false;
            }

            string name = headerLine.Substring(0, colonIndex).Trim();
            string value = headerLine.Substring(colonIndex + 1).Trim();

            // Check for valid characters
            // https://developers.cloudflare.com/rules/transform/request-header-modification/reference/header-format/

            // Check if name contains only valid characters
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9\-_]+$"))
            {
                _eventService.LogEvent(EventType.Warning, $"{headerLine}", null);
                _eventService.LogEvent(EventType.Warning, $"Couldn't process Header line: Contains unsupported characters", null);
                return false;
            }

            // Check if value is not empty and contains only valid characters
            if (string.IsNullOrEmpty(value) || !Regex.IsMatch(name, @"^[a-zA-Z0-9_:;.,\\/""'?!()\{\}\[\]@<>=\-+\*\#$&`|~\^%]*$"))
            {
                _eventService.LogEvent(EventType.Warning, $"{headerLine}", null);
                _eventService.LogEvent(EventType.Warning, $"Couldn't process Header line: Empty value or unsupported characters", null);
                return false;
            }

            return true;
        }
    }
}