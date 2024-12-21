using MTCG.DAL;
using MTCG.HTTP;

namespace MTCG
{
    internal class Program
    {
        private static readonly ServerService ServerService = new ServerService();

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

            // Listen for incoming Connections
            while (true)
            {
                ServerService.AcceptConnections();
            }

            // TODO: Implement correct way to stop the server, wait for all running tasks after exiting while loop:
            Task.WhenAll().Wait();
        }
    }
}

