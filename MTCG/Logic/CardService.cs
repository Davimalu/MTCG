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

        private static readonly Random random = new Random();

        public Card GetRandomCard()
        {
            // Generate random card

            int damage = random.Next(1, 101);
            ElementType type = (ElementType)random.Next(0, 3);

            // Either monster or spell card
            if (random.Next() % 2 == 0)
            {

                return new MonsterCard(GenerateCardName(), damage, type);
            }
            else
            {
                return new SpellCard(GenerateCardName(), damage, type);
            }
        }

        #region uniqueCardNames

        // AI generated card names
        private static readonly string[] adjectives = {
        "Fiery", "Icy", "Thunderous", "Shadowy", "Radiant", "Mystic", "Toxic", "Ancient",
        "Celestial", "Chaotic", "Ethereal", "Feral", "Spectral", "Arcane", "Primal"
        };

        private static readonly string[] nouns = {
        "Dragon", "Wizard", "Knight", "Beast", "Golem", "Specter", "Elemental", "Titan",
        "Phoenix", "Hydra", "Chimera", "Behemoth", "Leviathan", "Colossus", "Wyrm"
        };

        private static readonly string[] titles = {
        "of Doom", "the Undefeated", "of the Abyss", "the Enlightened", "of Legends",
        "the Destroyer", "of the Cosmos", "the Eternal", "of Whispers", "the Unyielding"
        };

        public static string GenerateCardName()
        {
            string adjective = adjectives[random.Next(adjectives.Length)];
            string noun = nouns[random.Next(nouns.Length)];
            string title = titles[random.Next(titles.Length)];

            return $"{adjective} {noun} {title}";
        }

        #endregion
    }
}
