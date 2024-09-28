namespace MTCG.Models
{
    public class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        private int elo;
        private int coinCount;

        private Stack stack;
        private Deck deck;

        // Constructor
        public User(string username, string password)
        {
            this.Username = username;
            this.Password = password;
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