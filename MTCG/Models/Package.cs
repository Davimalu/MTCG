using MTCG.Logic;

namespace MTCG.Models
{
    internal class Package
    {
        private List<Card> cards = new List<Card>(5); // A package consists of 5 cards
        private int price = 5; // The price of a package is 5 coins

        private CardService CardService = new CardService();

        // Constructor
        public Package()
        {
            // Fill package with 5 random cards
            for (int i = 0; i < 5; i++)
            {
                cards.Add(CardService.GetRandomCard());
            }
        }

        public List<Card> GetCards()
        {
            return cards;
        }
    }
}