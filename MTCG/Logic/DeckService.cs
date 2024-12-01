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
        /// <summary>
        /// adds a card to the user's deck
        /// </summary>
        /// <param name="card">the card to add</param>
        /// <param name="deck">the deck of the user</param>
        /// <returns>
        /// <para>true if card was added to deck</para>
        /// <para>false if card couldn't be added to deck (e.g. because there were already 4 cards in the deck)</para>
        /// </returns>
        public bool AddCardToUserDeck(Card card, Deck deck)
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
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[WARNING] Couldn't add card with ID {card.Id} to deck");
                Console.WriteLine($"[WARNING] {ex.Message}");
                Console.ResetColor();
                return false;
            }
        }
        
        /// <summary>
        /// removes a card from the user's deck
        /// </summary>
        /// <param name="cardToRemove">the card to remove</param>
        /// <param name="deck">the deck of the user</param>
        public void RemoveCardFromUserDeck(Card cardToRemove, Deck deck)
        {
            deck.Cards.RemoveAll(card => card.Id == cardToRemove.Id);
        }
    }
}
