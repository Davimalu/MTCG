using MTCG.Models.Enums;

namespace MTCG.Models.Cards
{
    public class MonsterCard : Card
    {
        public MonsterCard(string id, string name, float damage, ElementType elementType)
        {
            this.Id = id;
            this.Name = name;
            this.Damage = damage;
            this.TemporaryDamage = damage;
            this.ElementType = elementType;
        }

        public MonsterCard()
        {

        }

        // Copy Constructor
        public MonsterCard(Card card)
        {
            Id = card.Id;
            Name = card.Name;
            Damage = card.Damage;
            TemporaryDamage = card.TemporaryDamage;
            ElementType = card.ElementType;
        }
    }
}
