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
            bool success = AuthService.Register("testUser", "testPassword");
            bool success2 = AuthService.Register("testUser", "testPassword");

            bool loginSuccess = AuthService.Login("testUser", "testPassword");
            bool loginSuccess2 = AuthService.Login("testUser", "wrongPassword");

            User testUser = new User("testUser", "testPassword");

            UserService.BuyPackageForUser(testUser);
            UserService.printStackOfUser(testUser);

            (HTTPHeader headers, string? body) = HTTPService.AcceptConnection();
        }
    }
}

