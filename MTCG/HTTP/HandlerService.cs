using MTCG.Endpoints;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.HTTP
{
    public class HandlerService
    {
        private readonly Dictionary<string, IHttpEndpoint> _endpoints = new Dictionary<string, IHttpEndpoint>();

        private readonly HttpHeaderService _headerService = new HttpHeaderService();
        private readonly HttpBodyService _bodyService = new HttpBodyService();
        private readonly HttpResponseService _responseService = new HttpResponseService();

        private readonly IEventService _eventService = new EventService();

        public HandlerService()
        {
            // Add endpoints
            _endpoints.Add("/users", new UsersEndpoint());
            _endpoints.Add("/sessions", new SessionsEndpoint());
            _endpoints.Add("/packages", new PackagesEndpoint());
            _endpoints.Add("/transactions/packages", new TransactionsEndpoint());
            _endpoints.Add("/cards", new CardsEndpoint());
            _endpoints.Add("/deck", new DeckEndpoint());
            _endpoints.Add("/stats", new StatsEndpoint());
            _endpoints.Add("/scoreboard", new ScoreboardEndpoint());
            _endpoints.Add("/battles", new BattlesEndpoint());
            _endpoints.Add("/tradings", new TradingsEndpoint());
        }

        public void HandleClient(TcpClient client)
        {
            using StreamReader reader = new StreamReader(client.GetStream());

            using StreamWriter writer = new StreamWriter(client.GetStream())
            {
                AutoFlush = true
            };

            HTTPHeader? headers = _headerService.ParseHttpHeader(reader);
            if (headers == null)
            {
                _responseService.SendResponseToClient(writer, 400, JsonSerializer.Serialize("Invalid HTTP Header"));
                return;
            }

            _eventService.LogEvent(EventType.Info, $"HTTP Version: {headers.Version}, Path: {headers.Path}, Method: {headers.Method}", null);

            string? body = _bodyService.ParseHttpBody(reader, headers);

            // === Handle Request ===
            int responseCode = 404;
            string? responseBody = JsonSerializer.Serialize("Not found");

            // Get request path without query parameters | e.g. /deck?format=plain -> /deck
            string path = _headerService.GetPathWithoutQueryParameters(headers);

            // Only look up the first "directory" of the path in the dictionary | e.g. /users/kienboec -> /users
            var matchingKey = _endpoints.Keys.FirstOrDefault(key =>
                path.StartsWith(key));

            // Ensure that only one request of a specific user is processed at a time
            LockUser(headers);

            // Answer the request
            if (matchingKey != null)
            {
                try
                {
                    // Look up the path in the endpoints dictionary
                    var endpoint = _endpoints[matchingKey];
                    (responseCode, responseBody) = endpoint.HandleRequest(client, headers, body);
                }
                catch (Exception ex)
                {
                    responseBody = ex.ToString();
                }
            }

            // User can connect again now
            UnlockUser(headers);

            _responseService.SendResponseToClient(writer, responseCode, responseBody);
        }

        /// <summary>
        /// <para>if another request by this specific user (identified by Authorization Token) is currently being processed, this function blocks until that request is processed</para>
        /// <para>-> ensures that only one request of a specific user is processed at a time</para>
        /// </summary>
        /// <param name="headers"></param>
        private void LockUser(HTTPHeader headers)
        {
            lock (ThreadSync.UserLock)
            {
                if (headers.Headers.TryGetValue("Authorization", out var authorization))
                {
                    while (ThreadSync.ConnectedUsers.ContainsKey(authorization))
                    {
                        Monitor.Wait(ThreadSync.UserLock);
                    }

                    ThreadSync.ConnectedUsers[authorization] = true;
                }
            }
        }


        /// <summary>
        /// <para>has to be called once a request has finished processing</para>
        /// <para>-> signals other waiting threads that they can continue processing the request of a specific user</para>
        /// </summary>
        /// <param name="headers"></param>
        private static void UnlockUser(HTTPHeader headers)
        {
            lock (ThreadSync.UserLock)
            {
                if (headers.Headers.ContainsKey("Authorization") && headers.Headers.TryGetValue("Authorization", out var authorization))
                {
                    ThreadSync.ConnectedUsers.Remove(authorization, out _);

                    // Notify waiting threads
                    Monitor.PulseAll(ThreadSync.UserLock);
                }
            }
        }
    }
}
