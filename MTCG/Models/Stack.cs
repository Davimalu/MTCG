using MTCG.Models.Cards;

namespace MTCG.Models
{
    public class Stack
    {
        public List<Card> Cards { get; set; }

        // Constructor
        public Stack()
        {
            // Initialize empty stack
            Cards = new List<Card>();
        }
    }
}