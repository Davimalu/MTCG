using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

using MTCG.Models;
using MTCG.Logic;
using MTCG.HTTP;
using MTCG.DAL;

using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MTCG
{
    internal class Program
    {
        private static ServerService ServerService = new ServerService();

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

            while (true)
            {
                ServerService.AcceptConnections();
            }

            // TODO: Implement correct way to stop the server, wait for all running tasks after exiting while loop:
            Task.WhenAll().Wait();
        }
    }
}

