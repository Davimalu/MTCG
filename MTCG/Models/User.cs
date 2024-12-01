namespace MTCG.Models
{
    public class User
    {
        public int Id { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public int EloPoints { get; set; } = 100;
        public Stack Stack { get; set; } = new Stack();
        public Deck Deck { get; set; } = new Deck();

        private int _coinCount = 20;
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

        // Constructor
        public User(string username, string password)
        {
            this.Username = username;
            this.Password = password;
            EloPoints = 100;
            _coinCount = 20;
        }

        public User()
        {
            EloPoints = 100;
            _coinCount = 20;
        }
    }
}