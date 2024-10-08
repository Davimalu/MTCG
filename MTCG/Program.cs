using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

using MTCG.Models;
using MTCG.Logic;
using MTCG.HTTP;

using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MTCG
{
    internal class Program
    {
        public static AuthService AuthService = new AuthService();
        public static UserService UserService = new UserService();
        public static HTTPService HTTPService = new HTTPService();

        static void Main(string[] args)
        {
            while (true)
            {
                (HTTPHeader headers, string? body) = HTTPService.AcceptConnection();
            }
        }
    }
}

