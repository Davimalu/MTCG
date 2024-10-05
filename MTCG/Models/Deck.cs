namespace MTCG.Models
{
    public class Deck
    {
        public List<Card> Cards
        {
            get
            {
                return Cards;
            }
            set
            {
                // Check if deck has more than 4 cards
                if (value.Count > 4)
                {
                    throw new System.ArgumentException("Deck can only have 4 cards");
                }
                else
                {
                    Cards = value;
                }
            }
        }

        // Constructor
        public Deck()
        {
            // Initalize empty deck
            Cards = new List<Card>();
        }
    }
}