using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

using MTCG.Models;
using MTCG.Logic;
using MTCG.HTTP;
using MTCG.DAL;

using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MTCG
{
    internal class Program
    {
        private static ServerService ServerService = new ServerService();

        static void Main(string[] args)
        {
            // Create database tables
            DatabaseInitializer initializer = new DatabaseInitializer();
            initializer.CreateTables();

            while (true)
            {
                ServerService.AcceptConnections();
            }
        }
    }
}

