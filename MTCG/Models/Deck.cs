using System.Text.Json.Serialization;

namespace MTCG.Models
{
    public class Deck
    {
        [JsonIgnore]
        public int? Id { get; set; }
        private List<Card> _cards = new List<Card>();

        public List<Card> Cards
        {
            get => _cards;
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
    }
}