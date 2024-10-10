using MTCG.Endpoints;
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
        private static AuthService AuthService = new AuthService();
        private Dictionary<string, IHttpEndpoint> endpoints = new Dictionary<string, IHttpEndpoint>();
        private HTTPService HTTPService = new HTTPService();

        public HandlerService()
        {
            // Add endpoints
            endpoints.Add("/users", new UsersEndpoint());
            endpoints.Add("/sessions", new SessionsEndpoint());
        }

        public void HandleClient(TcpClient client)
        {
            using StreamReader reader = new StreamReader(client.GetStream());

            using StreamWriter writer = new StreamWriter(client.GetStream())
            {
                AutoFlush = true
            };

            HTTPHeader headers = HTTPService.ParseHTTPHeader(reader);
            string? body = HTTPService.ParseHTTPBody(reader, headers);

            // === Handle Request ===

            // Handle empty body
            if (body == null)
            {
                HTTPService.SendResponseToClient(writer, 400, "No data provided");
                return;
            }

            // Handle invalid JSON
            try
            {
                JsonSerializer.Deserialize<User>(body);
            }
            catch (JsonException E)
            {
                HTTPService.SendResponseToClient(writer, 400, E.Message);
                return;
            }

            int responseCode = 400;
            string? responseBody = null;

            try
            {
                var endpoint = endpoints[headers.Path];
                (responseCode, responseBody) = endpoint.HandleRequest(headers.Method, body, AuthService);
            }
            catch
            {
                responseBody = null;
            }

            HTTPService.SendResponseToClient(writer, responseCode, responseBody);
        }
    }
}
