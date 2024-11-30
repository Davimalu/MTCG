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
        #region Singleton
        private static CardService? _instance;

        public static CardService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CardService();
                }

                return _instance;
            }
        }
        #endregion

        public string GetCardType(Card card)
        {
            // Determine whether the card is a Monster or a Spell Card from the name of the card
            if (card.Name.Contains("Spell"))
            {
                return "Spell";
            }
            else
            {
                return "Monster";
            }
        }

        public string GetElementType(Card card)
        {
            // Determine the element type of the card from the name of the card
            if (card.Name.Contains("Water"))
            {
                return "Water";
            }
            else if (card.Name.Contains("Fire"))
            {
                return "Fire";
            }
            else
            {
                return "Normal";
            }
        }
    }
}
