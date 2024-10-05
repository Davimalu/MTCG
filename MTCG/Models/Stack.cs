namespace MTCG.Models
{
    public class Stack
    {
        public List<Card> Cards { get; set; }

        // Constructor
        public Stack()
        {
            // Initalize empty stack
            Cards = new List<Card>();
        }
    }
}