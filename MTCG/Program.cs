using MTCG.DAL;
using MTCG.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models.Enums;

namespace MTCG
{
    internal class Program
    {
        private static readonly ServerService ServerService = new ServerService();
        private static readonly ManualResetEventSlim ShutdownEvent = new ManualResetEventSlim(false);
        private static readonly IEventService _eventService = new EventService();

        static void Main(string[] args)
        {
            // Connect to database
            DatabaseService databaseService = DatabaseService.Instance;

            if (!databaseService.ConnectToDatabase())
            {
                Environment.Exit(-1);
            }

            // Create database tables
            DatabaseInitializer initializer = new DatabaseInitializer();
            initializer.CreateTables();

            // Start Server
            if (!ServerService.StartServer())
            {
                Environment.Exit(-2);
            }

            // Handle graceful shutdown
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                _eventService.LogEvent(EventType.Highlight, $"Shutdown signal received...", null);
                eventArgs.Cancel = true; // Prevent immediate termination
                ShutdownEvent.Set(); // Signal shutdown
                ServerService.StopServer(); // Shutdown server
            };

            // Listen for incoming Connections
            while (!ShutdownEvent.IsSet)
            {
                ServerService.AcceptConnections();
            }

            Task.WhenAll().Wait(); // Wait for all running tasks to complete
        }
    }
}

