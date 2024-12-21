using MTCG.Models;

namespace MTCG.Interfaces.HTTP;

public interface IHttpHeaderService
{
    /// <summary>
    /// Parses the HTTP Header of a Request
    /// </summary>
    /// <param name="reader">A StreamReader for reading the corresponding TCPClient's Network Stream</param>
    /// <returns>All HTTP Header information parsed as the HttpHeader Class</returns>
    public HTTPHeader? ParseHttpHeader(StreamReader reader);
    /// <summary>
    /// extracts the Authorization token from the HTTP Header, e.g. Bearer admin-mtcgToken -> admin-mtcgToken
    /// </summary>
    /// <param name="headers">already initialized and filled HttpHeader Class</param>
    /// <returns>
    /// <para>the Authorization token</para>
    /// <para>null if not present or invalid</para>
    /// </returns>
    string? GetTokenFromHeader(HTTPHeader headers);
    /// <summary>
    /// checks if the Authorization Header value matches the format required by this application, e.g. Bearer admin-mtcgToken
    /// </summary>
    /// <param name="token">value of the Authorization field in the HTTP Header</param>
    /// <returns></returns>
    bool IsValidAuthorizationField(string token);

    /// <summary>
    /// parse all query Parameters of the HTTP request
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