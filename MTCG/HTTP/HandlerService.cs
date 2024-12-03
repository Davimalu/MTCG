﻿using MTCG.Endpoints;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public HandlerService()
        {
            // Add endpoints
            _endpoints.Add("/users", new UsersEndpoint());
            _endpoints.Add("/sessions", new SessionsEndpoint());
            _endpoints.Add("/packages", new PackagesEndpoint());
            _endpoints.Add("/transactions/packages", new TransactionsEndpoint());
            _endpoints.Add("/cards", new CardsEndpoint());
            _endpoints.Add("/deck", new DeckEndpoint());
        }

        public void HandleClient(TcpClient client)
        {
            using StreamReader reader = new StreamReader(client.GetStream());

            using StreamWriter writer = new StreamWriter(client.GetStream())
            {
                AutoFlush = true
            };

            HTTPHeader headers = _httpService.ParseHTTPHeader(reader);
            string? body = _httpService.ParseHTTPBody(reader, headers);

            Console.WriteLine($"[INFO] Client connected: {headers.Version} {headers.Path} {headers.Method}");

            // === Handle Request ===
            int responseCode = 400;
            string? responseBody = null;

            // Get request path without query parameters | e.g. /deck?format=plain -> /deck
            string path = HeaderHelper.GetPathWithoutQueryParameters(headers);

            // Only look up the first "directory" of the path in the dictionary | e.g. /users/kienboec -> /users
            var matchingKey = _endpoints.Keys.FirstOrDefault(key =>
                path.StartsWith(key));

            if (matchingKey != null)
            {
                try
                {
                    // Look up the path in the endpoints dictionary
                    var endpoint = _endpoints[matchingKey];
                    (responseCode, responseBody) = endpoint.HandleRequest(headers, body);
                }
                catch (Exception ex)
                {
                    responseBody = ex.ToString();
                }
            }

            _httpService.SendResponseToClient(writer, responseCode, responseBody);
        }
    }
}
