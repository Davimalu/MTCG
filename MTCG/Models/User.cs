using System.Text.Json;
using System.Text.Json.Serialization;

namespace MTCG.Models
{
    public class User
    {
        public int? Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public UserStatistics Stats { get; set; } = new UserStatistics();
        public string Password { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
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
            _coinCount = 20;
        }

        public User()
        {
            _coinCount = 20;
        }
    }
}