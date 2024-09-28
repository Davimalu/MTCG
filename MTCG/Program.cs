using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using MTCG.Models;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            User testUser = new User("testUser", "testPassword");

            testUser.BuyPackage();
            testUser.printStack();
        }
    }
}

