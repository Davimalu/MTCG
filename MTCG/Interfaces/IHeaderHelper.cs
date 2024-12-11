using MTCG.Models;

namespace MTCG.HTTP;

public interface IHeaderHelper
{
    string? GetTokenFromHeader(HTTPHeader headers);
    bool IsValidAuthorizationField(string token);

    /// <summary>
    /// parse all query Parameters that were sent with the HTTP request
    /// </summary>
    /// <param name="headers"></param>
    /// <returns>returns a dictionary containing all query parameters as key value pairs</returns>
    Dictionary<string, string> GetQueryParameters(HTTPHeader headers);

    /// <summary>
    /// removes all query parameters from a HTTP Request Path
    /// </summary>
    /// <param name="headers"></param>
    /// <returns>the Path without any query parameters, e.g. /deck?format=plain -> /deck</returns>
    string GetPathWithoutQueryParameters(HTTPHeader headers);
}