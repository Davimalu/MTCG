namespace MTCG.Models
{
    internal class Deck
    {
        private List<Card> cards = new List<Card>();

        // Constructor
        public Deck()
        {

        }
        public bool AddCard(Card card)
        {
            if (cards.Count < 4)
            {
                cards.Add(card);
                return true;
            } else
            {
                return false;
            }
        }

        public bool RemoveCard(Card card)
        {
            if (cards.Contains(card))
            {
                cards.Remove(card);
                return true;
            }
            else
            {
                return false;
            }
        }

        public List<Card> GetCards()
        {
            return cards;
        }
    }
}