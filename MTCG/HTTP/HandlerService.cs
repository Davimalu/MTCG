using MTCG.Endpoints;
using MTCG.Interfaces.HTTP;
using MTCG.Logic;
using MTCG.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCG.HTTP
{
    public class HandlerService
    {
        private static AuthService _authService = new AuthService();
        private Dictionary<string, IHttpEndpoint> _endpoints = new Dictionary<string, IHttpEndpoint>();
        private HTTPService _httpService = new HTTPService();
        private readonly HttpHeaderService _headerService = new HttpHeaderService();

        private readonly IHttpHeaderService _ihttpHeaderService = new HttpHeaderService();

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

            HTTPHeader headers = _headerService.ParseHttpHeader(reader);
            string? body = _httpService.ParseHTTPBody(reader, headers);

            Console.WriteLine($"[INFO] Client connected: {headers.Version} {headers.Path} {headers.Method}");

            // === Handle Request ===
            int responseCode = 404;
            string? responseBody = JsonSerializer.Serialize("Not found");

            // Get request path without query parameters | e.g. /deck?format=plain -> /deck
            string path = _ihttpHeaderService.GetPathWithoutQueryParameters(headers);

            // Only look up the first "directory" of the path in the dictionary | e.g. /users/kienboec -> /users
            var matchingKey = _endpoints.Keys.FirstOrDefault(key =>
                path.StartsWith(key));


            // Ensure that only one request of a specific user is processed at a time
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
            lock (ThreadSync.UserLock)
            {
                if (headers.Headers.ContainsKey("Authorization") && headers.Headers.TryGetValue("Authorization", out var authorization))
                {
                    ThreadSync.ConnectedUsers.Remove(authorization, out _);

                    // Notify waiting threads
                    Monitor.PulseAll(ThreadSync.UserLock);
                }
            }

            _httpService.SendResponseToClient(writer, responseCode, responseBody);
        }
    }
}
