using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

using MTCG.Models;
using MTCG.Logic;

namespace MTCG
{
    internal class Program
    {
        public static AuthService AuthService = new AuthService();

        static void Main(string[] args)
        {
            bool success = AuthService.Register("testUser", "testPassword");
            bool success2 = AuthService.Register("testUser", "testPassword");

            bool loginSuccess = AuthService.Login("testUser", "testPassword");
            bool loginSuccess2 = AuthService.Login("testUser", "wrongPassword");

            User testUser = new User("testUser", "testPassword");

            testUser.BuyPackage();
            testUser.printStack();
        }
    }
}

