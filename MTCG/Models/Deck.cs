namespace MTCG.Models
{
    public class Deck
    {
        private List<Card> _cards;

        public List<Card> Cards
        {
            get
            {
                return _cards;
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
                    _cards = value;
                }
            }
        }

        // Constructor
        public Deck()
        {
            // Initalize empty deck
            _cards = new List<Card>();
        }
    }
}