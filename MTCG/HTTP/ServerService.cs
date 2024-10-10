using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.Endpoints;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;

namespace MTCG.HTTP
{
    public class ServerService
    {
        private HandlerService HandlerService = new HandlerService();

        private TcpListener server;
        
        // Constructor
        public ServerService()
        {
            // Start server
            server = new TcpListener(IPAddress.Any, 10001);
            server.Start();

            Console.WriteLine("Server started!");
        }

        public void AcceptConnections()
        {
            // Accept client
            Console.WriteLine("Waiting for clients...");
            var client = server.AcceptTcpClient();
            Console.WriteLine("Client connected.");

            HandlerService.HandleClient(client);
        }

        
    }
}
