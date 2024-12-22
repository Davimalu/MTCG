using MTCG.Models.Enums;

namespace MTCG.Models.Cards
{
    public class SpellCard : Card
    {
        public SpellCard(string id, string name, float damage, ElementType elementType)
        {
            this.Id = id;
            this.Name = name;
            this.Damage = damage;
            this.TemporaryDamage = damage;
            this.ElementType = elementType;
        }

        public SpellCard() { }

        // Copy Constructor
        public SpellCard(Card card)
        {
            Id = card.Id;
            Name = card.Name;
            Damage = card.Damage;
            TemporaryDamage = card.TemporaryDamage;
            ElementType = card.ElementType;
        }
    }
}
