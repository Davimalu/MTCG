namespace MTCG.Models
{
    public class User
    {
        private string username;
        private string password;
        private int elo;
        private int coinCount;

        private Stack stack;
        private Deck deck;

        // Constructor
        public User(string username, string password)
        {
            this.username = username;
            this.password = password;
            elo = 0;
            coinCount = 20;

            stack = new Stack();
            deck = new Deck();
        }

        public bool BuyPackage()
        {
            if (coinCount >= 5)
            {
                coinCount -= 5;

                Package package = new Package();
                stack.AddPackageToStack(package);

                return true;
            }
            else
            {
                return false;
            }
        }

        public void printStack()
        {
            List<Card> cards = stack.GetCards();
            foreach (Card card in cards)
            {
                card.printCard();
            }
        }
    }
}