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
        private int PortNo = 10001;
        
        // Constructor
        public ServerService()
        {
            // Start server
            server = new TcpListener(IPAddress.Any, PortNo);
            server.Start();

            Console.WriteLine("[INFO] Server started!");
        }

        public void AcceptConnections()
        {
            // Accept client
            Console.WriteLine($"[INFO] Listening on http://localhost:{PortNo}...");
            var client = server.AcceptTcpClient();

            // Get client IP Address
            IPEndPoint remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine($"[INFO] Received a request from {remoteIpEndPoint.Address}");

            // Start new thread for client
            Console.WriteLine($"[INFO] Starting new thread for request...");
            Task.Run(() => HandlerService.HandleClient(client));

            // TODO: Go through the whole code again and guard critical sections
        }
    }
}
