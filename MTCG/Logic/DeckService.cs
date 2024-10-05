using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;

namespace MTCG.Logic
{
    public class DeckService
    {
        public bool AddCardToDeck(Card card, Deck deck)
        {
            try
            {
                if (deck.Cards.Count >= 4)
                {
                    throw new ArgumentException("Deck can only have 4 cards");
                }

                deck.Cards.Add(card);
                return true;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public bool RemoveCardFromDeck(Card card, Deck deck)
        {
            if (deck.Cards.Contains(card))
            {
                deck.Cards.Remove(card);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
