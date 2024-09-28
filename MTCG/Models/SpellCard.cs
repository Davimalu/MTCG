using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class SpellCard : Card
    {
        public SpellCard(string name, int damage, ElementType elementType) : base(name, damage, elementType)
        {

        }

        public override void printCard()
        {
            Console.WriteLine($"Name: {name}");
            Console.WriteLine($"Damage: {damage}");
            Console.WriteLine($"Element Type: {elementType}");
            Console.WriteLine("Card Type: Spell");
        }
    }
}
