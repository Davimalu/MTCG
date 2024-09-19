using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }

        // Classes
        User testUser = new User("testUser", "testPassword");

        // a user can manage his cards
        Card testCard = new Card("testCard", 10, ElementType.Fire, CardType.Monster);

        // a user can buy cards by acquiring packages
        Package testPackage = new Package();
    }
}

