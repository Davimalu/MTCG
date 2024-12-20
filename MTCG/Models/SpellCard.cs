using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class SpellCard : Card
    {
        public SpellCard(string id, string name, float damage, ElementType elementType) : base(id, name, damage, elementType)
        {

        }

        // Copy Constructor
        public SpellCard(Card card)
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
            Console.WriteLine("Card Type: Spell");
        }
    }
}
