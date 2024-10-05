namespace MTCG.Models
{
    public class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public Stack Stack { get; private set; }
        public Deck Deck { get; private set; }
        public int CoinCount
        {
            get
            {
                return _coinCount;
            }
            set
            {
                // Check if coin count is negative
                if (value < 0)
                {
                    throw new System.ArgumentException("Coin count cannot be negative");
                }
                else
                {
                    _coinCount = value;
                }
            }
        }

        private int elo;
        private int _coinCount;

        // Constructor
        public User(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            elo = 0;
            _coinCount = 20;

            // Initalize empty stack and deck
            Stack = new Stack();
            Deck = new Deck();
        }
    }
}