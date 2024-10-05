using MTCG.Logic;

namespace MTCG.Models
{
    public class Package
    {
        public List<Card> Cards { get; private set; }
        private int price = 5; // The price of a package is 5 coins

        // Constructor
        public Package()
        {
            // Initalize empty package
            Cards = new List<Card>(5);
        }

    }
}