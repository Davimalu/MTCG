using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models.Enums;
using System.Net;
using System.Net.Sockets;

namespace MTCG.HTTP
{
    public class ServerService
    {
        private const int PORT_NO = 10001;

        private readonly HandlerService _handlerService = new HandlerService();
        private readonly IEventService _eventService = new EventService();

        private TcpListener? _server;


        public bool StartServer()
        {
            try
            {
                // Start server
                _server = new TcpListener(IPAddress.Any, PORT_NO);
                _server.Start();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, $"Couldn't start server", ex);
                return false;
            }

            _eventService.LogEvent(EventType.Highlight, $"Server started on Port {PORT_NO}", null);
            return true;
        }


        public void StopServer()
        {
            if (_server != null)
            {
                _server.Stop();
            }
        }


        public void AcceptConnection()
        {
            // Check if server is running
            if (_server == null)
            {
                _eventService.LogEvent(EventType.Warning, $"Cannot listen for incoming connections: Server has not yet been started", null);
                return;
            }

            // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener.localendpoint?view=net-9.0
            _eventService.LogEvent(EventType.Info, $"Listening for connections on {IPAddress.Parse(((IPEndPoint)_server.LocalEndpoint).Address.ToString())}:{((IPEndPoint)_server.LocalEndpoint).Port.ToString()}...", null);

            // Try to accept a connection
            TcpClient? client = null;
            try
            {
                client = _server.AcceptTcpClient();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, $"Error accepting connection", ex);
                return;
            }

            // Get client IP Address
            IPEndPoint? remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            _eventService.LogEvent(EventType.Info, $"Received a request from {remoteIpEndPoint?.Address}", null);

            // Start new thread for client
            _eventService.LogEvent(EventType.Info, $"Starting new thread for request...", null);
            Task.Run(() => _handlerService.HandleClient(client));
        }
    }
}
