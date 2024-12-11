using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.HTTP
{
    public class HeaderHelper : IHeaderHelper
    {
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
            string regex = @"^Bearer\s+\w+-mtcgToken$"; // https://regex101.com/r/iTbKSU/1
            return Regex.IsMatch(token, regex);
        }

        /// <summary>
        /// parse all query Parameters that were sent with the HTTP request
        /// </summary>
        /// <param name="headers"></param>
        /// <returns>returns a dictionary containing all query parameters as key value pairs</returns>
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

        /// <summary>
        /// removes all query parameters from a HTTP Request Path
        /// </summary>
        /// <param name="headers"></param>
        /// <returns>the Path without any query parameters, e.g. /deck?format=plain -> /deck</returns>
        public string GetPathWithoutQueryParameters(HTTPHeader headers)
        {
            if (headers.Path.Contains('?'))
            {
                return headers.Path.Split('?')[0];
            }

            return headers.Path;
        }
    }
}
