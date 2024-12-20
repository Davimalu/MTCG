using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class MonsterCard : Card
    {
        public MonsterCard(string id, string name, float damage, ElementType elementType) : base(id, name, damage, elementType)
        {

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
            ElementType = card.ElementType;
        }

        public override void PrintCard()
        {
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Damage: {Damage}");
            Console.WriteLine($"Element Type: {ElementType}");
            Console.WriteLine("Card Type: Monster");
        }
    }
}
