using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;

namespace MTCG.HTTP
{
    public class HttpBodyService
    {
        private readonly IEventService _eventService = new EventService();

        public HttpBodyService() { }

        #region DependencyInjection
        public HttpBodyService(IEventService eventService)
        {
            _eventService = eventService;
        }
        #endregion

        public string? ParseHttpBody(StreamReader reader, HTTPHeader headers)
        {
            if (headers.Headers.TryGetValue("Content-Length", out var contentLengthValue))
            {
                int contentLength;
                try
                {
                    contentLength = int.Parse(contentLengthValue);
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Warning, $"Couldn't parse HTTP Request Body: Invalid Content Length", ex);
                    return null;
                }

                if (contentLength < 0)
                {
                    _eventService.LogEvent(EventType.Warning, $"Couldn't parse HTTP Request Body: Invalid Content Length", null);
                    return null;
                }

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
    }
}
