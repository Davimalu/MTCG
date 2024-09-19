using static MTCG.Card;

namespace MTCG
{
    // Enum for element type
    public enum ElementType
    {
        Fire,
        Water,
        Normal
    }

    // Enum for card type
    public enum CardType
    {
        Monster,
        Spell
    }

    internal class Card
    {
        private string name;
        private readonly int damage; // The damage of a card is constant and does not change
        private ElementType elementType;
        private CardType cardType;

        public Card(string name, int damage, ElementType elementType, CardType cardType)
        {
            this.name = name;
            this.damage = damage;
            this.elementType = elementType;
            this.cardType = cardType;
        }


    }
}