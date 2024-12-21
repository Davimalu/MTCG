namespace MTCG.HTTP;

public interface IHttpResponseService
{
    /// <summary>
    /// sends a well-formatted HTTP Response to the client
    /// </summary>
    /// <param name="writer">StreamWriter object associated with the TcpClient to which the response is to be sent</param>
    /// <param name="statusCode">Http Status Code</param>
    /// <param name="response">Http Response body</param>
    void SendResponseToClient(StreamWriter writer, int statusCode, string? response);

    bool IsJson(string? source);
}