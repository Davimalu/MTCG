using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;

namespace MTCG.Logic
{
    public class CardService
    {
        public CardService()
        {
        
        }

        public Card GetRandomCard()
        {
            // Generate random card
            Random random = new Random();

            int damage = random.Next(1, 101);
            ElementType type = (ElementType)random.Next(0, 3);

            // Either monster or spell card
            if (random.Next() % 2 == 0)
            {

                return new MonsterCard("Monster", damage, type);
            }
            else
            {
                return new SpellCard("Spell", damage, type);
            }
        }
    }
}
