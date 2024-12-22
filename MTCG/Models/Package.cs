using MTCG.Models.Cards;

namespace MTCG.Models
{
    public class Package
    {
        public int? Id { get; set; }
        public List<Card> Cards { get; set; }
        public int Price = 5;

        // Constructor
        public Package()
        {
            // Initialize empty package
            Cards = new List<Card>(5);
        }

    }
}