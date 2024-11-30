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

        public override void PrintCard()
        {
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Damage: {Damage}");
            Console.WriteLine($"Element Type: {ElementType}");
            Console.WriteLine("Card Type: Spell");
        }
    }
}
