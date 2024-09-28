namespace MTCG.Models
{
    internal class Stack
    {
        private List<Card> cards = new List<Card>();

        // Constructor
        public Stack()
        {
            
        }

        public void AddCard(Card card)
        {
            cards.Add(card);
        }

        public bool RemoveCard(Card card) {
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