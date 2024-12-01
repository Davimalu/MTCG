using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.HTTP
{
    public static class HeaderHelper
    {
        public static string? GetTokenFromHeader(HTTPHeader headers)
        {
            // Provided authorization string should be something like "Bearer admin-mtcgToken"

            // Check if authorization header was provided
            if (!headers.Headers.ContainsKey("Authorization"))
            {
                throw new ArgumentException("No Authorization Header provided!");
            }

            // Check for correct format of the authorization header
            if (!IsValidAuthorizationField(headers.Headers["Authorization"]))
            {
                throw new ArgumentException("Authorization Header is not in the correct format!");
            }

            // Return token
            return headers.Headers["Authorization"].Split(' ')[1];
        }

        public static bool IsValidAuthorizationField(string token)
        {
            // Use @ to ignore escape sequences in string: https://stackoverflow.com/questions/556133/whats-the-in-front-of-a-string-in-c
            string regex = @"^Bearer\s+\w+-mtcgToken$"; // https://regex101.com/r/iTbKSU/1
            return Regex.IsMatch(token, regex);
        }
    }
}
