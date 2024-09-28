namespace MTCG.Models
{
    internal class Deck
    {
        private Card[] cards;

        // Constructor
        public Deck()
        {
            // Initialize an empty deck
            cards = Array.Empty<Card>();
        }
    }
}