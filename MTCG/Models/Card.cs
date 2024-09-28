using static MTCG.Models.Card;

namespace MTCG.Models
{
    // Enum for element type
    public enum ElementType
    {
        Fire,
        Water,
        Normal
    }

    public abstract class Card
    {
        private string name;
        private readonly int damage; // The damage of a card is constant and does not change
        private ElementType elementType;

        public Card(string name, int damage, ElementType elementType)
        {
            this.name = name;
            this.damage = damage;
            this.elementType = elementType;
        }


    }
}