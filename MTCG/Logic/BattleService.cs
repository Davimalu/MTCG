using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Models;

namespace MTCG.Logic
{
    public class BattleService
    {
        private static Random _rnd = new Random();

        public User? StartBattle(User playerA, User playerB)
        {
            int counter = 0;

            // The fight continues as long as both players still have cards in their deck and less than 100 rounds have been played
            while (playerA.Deck.Cards.Count > 0 && playerB.Deck.Cards.Count > 0)
            {
                Card? playerACard = GetRandomCardFromDeck(playerA.Deck);
                Card? playerBCard = GetRandomCardFromDeck(playerB.Deck);

                if (playerACard == null || playerBCard == null)
                {
                    // This should never happen
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[ERROR] Fatal error during battle. Couldn't retrieve card from Player's deck");
                    Console.ResetColor();
                    return null;
                }

                Card? winnerCard = FightOneRound(playerACard, playerBCard);

                // Continue on draw
                if (winnerCard == null)
                {
                    continue;
                }

                // If A wins, transfer B's card to him
                if (playerA.Deck.Cards.Contains(winnerCard))
                {
                    playerA.Deck.Cards.Add(playerBCard);
                    playerB.Deck.Cards.Remove(playerBCard);
                }

                // If B wins, transfer A's card to him
                if (playerB.Deck.Cards.Contains(winnerCard))
                {
                    playerB.Deck.Cards.Add(playerACard);
                    playerA.Deck.Cards.Remove(playerACard);
                }

                counter++;
                // If the counter reaches 100, the fight is over
                if (counter >= 100)
                {
                    return null; // Draw
                }
            }

            if (playerA.Deck.Cards.Count > playerB.Deck.Cards.Count)
            {
                return playerA; // A wins
            }
            else if (playerB.Deck.Cards.Count > playerA.Deck.Cards.Count)
            {
                return playerB; // B wins
            }
            else
            {
                return null; // Draw
            }
        }

        /// <summary>
        /// performs a round of combat between two playing cards
        /// </summary>
        /// <param name="cardA"></param>
        /// <param name="cardB"></param>
        /// <returns>
        /// <para>returns the card that won the round</para>
        /// <para>returns null on draw</para>
        /// </returns>
        private Card? FightOneRound(Card cardA, Card cardB)
        {
            if (cardA is MonsterCard && cardB is MonsterCard)
            {
                // Pure Monster Fight (both cards are monsters) -> fights are not affected by the element type
                cardA.TemporaryDamage = cardA.Damage;
                cardB.TemporaryDamage = cardB.Damage;
                return CardFight(cardA, cardB);
            }
            else
            {
                // at least one of the cards is a spell card -> element type has an effect on the damage calculation of this single round
                cardA.TemporaryDamage = cardA.Damage;
                cardB.TemporaryDamage = cardB.Damage;

                // water is effective against fire, so damage is doubled
                // fire is not effective against water, so damage is halved
                if (cardA.ElementType == ElementType.Water && cardB.ElementType == ElementType.Fire)
                {
                    cardA.TemporaryDamage *= 2;
                    cardB.TemporaryDamage /= 2;
                }

                if (cardB.ElementType == ElementType.Water && cardA.ElementType == ElementType.Fire)
                {
                    cardB.TemporaryDamage *= 2;
                    cardA.TemporaryDamage /= 2;
                }

                // fire is effective against normal, so damage is doubled
                // normal is not effective against fire, so damage is halved
                if (cardA.ElementType == ElementType.Fire && cardB.ElementType == ElementType.Normal)
                {
                    cardA.TemporaryDamage *= 2;
                    cardB.TemporaryDamage /= 2;
                }

                if (cardB.ElementType == ElementType.Fire && cardA.ElementType == ElementType.Normal)
                {
                    cardB.TemporaryDamage *= 2;
                    cardA.TemporaryDamage /= 2;
                }

                // normal is effective against water, so damage is doubled
                // water is not effective against normal, so damage is halved
                if (cardA.ElementType == ElementType.Normal && cardB.ElementType == ElementType.Water)
                {
                    cardA.TemporaryDamage *= 2;
                    cardB.TemporaryDamage /= 2;
                }

                if (cardB.ElementType == ElementType.Normal && cardA.ElementType == ElementType.Water)
                {
                    cardB.TemporaryDamage *= 2;
                    cardA.TemporaryDamage /= 2;
                }

                return CardFight(cardA, cardB);
            }
        }

        private Card? CardFight(Card cardA, Card cardB)
        {
            if (cardA.TemporaryDamage > cardB.TemporaryDamage)
            {
                return cardA;
            }
            else if (cardB.TemporaryDamage > cardA.TemporaryDamage)
            {
                return cardB;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// retrieve a random card from a user's deck
        /// </summary>
        /// <param name="deck"></param>
        /// <returns>
        /// <para>a card object on success</para>
        /// <para>null if deck is empty or on error</para>
        /// </returns>
        private Card? GetRandomCardFromDeck(Deck deck)
        {
            int r = _rnd.Next(deck.Cards.Count);
            return deck.Cards[r];
        }
    }
}
