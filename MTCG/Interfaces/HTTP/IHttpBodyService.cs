using MTCG.Models;

namespace MTCG.HTTP;

public interface IHttpBodyService
{
    string? ParseHttpBody(StreamReader reader, HTTPHeader headers);
}