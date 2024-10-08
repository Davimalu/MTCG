using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using MTCG.Logic;
using MTCG.Models;

namespace MTCG.HTTP
{
    public class HTTPService
    {
        private static AuthService AuthService = new AuthService();
        
        private TcpListener server;
        // Connected clients
        private List<TcpClient> connectedClients = new List<TcpClient>();

        // Constructor
        public HTTPService()
        {
            // Start server
            server = new TcpListener(IPAddress.Any, 10001);
            server.Start();

            Console.WriteLine("Server started!");
        }

        public (HTTPHeader, string?) AcceptConnection()
        {
            Console.WriteLine("Waiting for clients...");
            var client = server.AcceptTcpClient();
            connectedClients.Add(client);
            Console.WriteLine("Client connected.");

            using StreamReader reader = new StreamReader(client.GetStream());

            using StreamWriter writer = new StreamWriter(client.GetStream())
            {
                AutoFlush = true
            };

            HTTPHeader headers = ParseHTTPHeader(reader);
            string? body = ParseHTTPBody(reader, headers);

            // === Handle Request ===

            // Handle empty body
            if (body == null)
            {
                SendResponseToClient(writer, 400, "No data provided");
                return (headers, body);
            }

            // Handle invalid JSON
            try
            {
                JsonSerializer.Deserialize<User>(body);
            }
            catch (JsonException E)
            {
                SendResponseToClient(writer, 400, E.Message);
                return (headers, body);
            }

            // User Registration
            if (headers.Method == "POST" && headers.Path == "/users")
            {
                User? tempUser = JsonSerializer.Deserialize<User>(body);

                // Check if Username and Password were provided
                if (tempUser == null)
                {
                    SendResponseToClient(writer, 400, "Invalid data provided");
                    return (headers, body);
                }

                // Try registering the user
                if (AuthService.Register(tempUser.Username, tempUser.Password))
                {
                    SendResponseToClient(writer, 201, "User Created");
                }
                else
                {
                    SendResponseToClient(writer, 400, "User already exists");
                }
            }

            // User Login
            if (headers.Method == "POST" && headers.Path == "/sessions")
            {
                User? tempUser = JsonSerializer.Deserialize<User>(body);

                // Check if Username and Password were provided
                if (tempUser == null)
                {
                    SendResponseToClient(writer, 400, "Invalid data provided");
                    return (headers, body);
                }

                string? token = AuthService.Login(tempUser.Username, tempUser.Password);

                if (token == null)
                {
                    SendResponseToClient(writer, 403, "Login failed");
                    return (headers, body);
                }
                else
                {
                    var jsonObject = new Dictionary<string, string>
                    {
                        { $"{tempUser.Username}-mtcgToken", token }
                    };

                    string jsonString = JsonSerializer.Serialize(jsonObject);

                    SendResponseToClient(writer, 200, jsonString);

                    return (headers, body);
                }
            }

            return (headers, body);
        }

        public void SendResponseToClient(StreamWriter writer, int statusCode, string response)
        {
            string reasonPhrase;

            switch(statusCode)
            {
                case 200:
                    reasonPhrase = "OK";
                    break;
                case 201:
                    reasonPhrase = "Created";
                    break;
                case 400:
                    reasonPhrase = "Bad Request";
                    break;
                case 401:
                    reasonPhrase = "Unauthorized";
                    break;
                case 403:
                    reasonPhrase = "Forbidden";
                    break;
                case 404:
                    reasonPhrase = "Not Found";
                    break;
                case 500:
                    reasonPhrase = "Internal Server Error";
                    break;
                default:
                    reasonPhrase = "Unknown";
                    break;
            }

            writer.WriteLine($"HTTP/1.1 {statusCode} {reasonPhrase}");
            writer.WriteLine("Content-Type: application/json");
            writer.WriteLine($"Content-Length: {response.Length}");
            writer.WriteLine();

            // HTTP Response Body
            writer.WriteLine(response);

            writer.Close();
            connectedClients[0].Close();
            connectedClients.RemoveAt(0);
        }

        private HTTPHeader ParseHTTPHeader(StreamReader reader)
        {
            string? line;
            line = reader.ReadLine();

            // Check if HTTP header is present
            if (line == null)
            {
                throw new Exception("No HTTP header found");
            }

            var httpParts = line.Split(' ');

            var method = httpParts[0];
            var path = httpParts[1];
            var version = httpParts[2];

            HTTPHeader headers = new HTTPHeader()
            {
                Method = method,
                Path = path,
                Version = version
            };

            while ((line = reader.ReadLine()) != null)
            {
                // Empty line indicates end of HTTP headers
                if (line.Length == 0)
                {
                    break;
                }

                // Parse header fields
                var headerParts = line.Split(":");

                var headerName = headerParts[0].Trim();
                var headerValue = headerParts[1].Trim();

                headers.Headers.Add(headerParts[0], headerParts[1]);
            }

            return headers;
        }

        private string? ParseHTTPBody(StreamReader reader, HTTPHeader headers)
        {
            if (headers.Headers.ContainsKey("Content-Length"))
            {
                int contentLength = int.Parse(headers.Headers["Content-Length"]);

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
