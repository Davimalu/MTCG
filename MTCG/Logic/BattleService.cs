using MTCG.Interfaces;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using System.Text.Json;
using MTCG.Models.Cards;
using MTCG.Models.Enums;

namespace MTCG.Logic
{
    public class BattleService : IBattleService
    {
        private static readonly Random Rnd = new Random();
        private readonly List<string> _battleLog = [];

        private readonly IUserService _userService = UserService.Instance;
        private readonly IEventService _eventService = new EventService();

        public BattleService() { }

        #region DependencyInjection
        public BattleService(IUserService userService, IEventService eventService)
        {
            _userService = userService;
            _eventService = eventService;
        }
        #endregion

        public string? StartBattle(User playerA, User playerB)
        {
            int counter = 1;

            // The fight continues as long as both players still have cards in their deck and less than 100 rounds have been played
            while (playerA.Deck.Cards.Count > 0 && playerB.Deck.Cards.Count > 0 && counter <= 100)
            {
                _battleLog.Add($">>> Round {counter} <<<");

                Card? playerACard = GetRandomCardFromDeck(playerA.Deck);
                Card? playerBCard = GetRandomCardFromDeck(playerB.Deck);

                if (playerACard == null || playerBCard == null)
                {
                    // This should never happen
                    _eventService.LogEvent(EventType.Error, $"Fatal error during battle: Couldn't retrieve card from Player's deck", null);
                    return null;
                }

                LogPlayedCards(playerA, playerB, playerACard, playerBCard);

                Card? winnerCard = FightOneRound(playerACard, playerBCard);

                // Continue on draw
                if (winnerCard == null)
                {
                    _battleLog.Add($"No one has won Round {counter} - it's a tie!");
                    counter++;
                    continue;
                }

                TransferCardFromLoserToWinner(playerA, playerB, winnerCard, counter, playerACard, playerBCard);

                counter++;
            }

            ProcessBattleOutcome(playerA, playerB, counter);
            return JsonSerializer.Serialize(_battleLog);
        }


        private void LogPlayedCards(User playerA, User playerB, Card playerACard, Card playerBCard)
        {
            _battleLog.Add($"{playerA.Username} plays Card {playerACard.Name}!");
            _battleLog.Add($"Card Type: {(playerACard is MonsterCard ? "Monster" : "Spell")} | Damage: {playerACard.Damage} | Element Type: {playerACard.ElementType.ToString()}");

            _battleLog.Add($"{playerB.Username} plays Card {playerBCard.Name}!");
            _battleLog.Add($"Card Type: {(playerBCard is MonsterCard ? "Monster" : "Spell")} | Damage: {playerBCard.Damage} | Element Type: {playerBCard.ElementType.ToString()}");
        }


        private void TransferCardFromLoserToWinner(User playerA, User playerB, Card winnerCard, int counter, Card playerACard, Card playerBCard)
        {
            // If A wins, transfer B's card to him
            if (playerA.Deck.Cards.Contains(winnerCard))
            {
                _battleLog.Add($"{playerA.Username} has won Round {counter}!");
                playerA.Deck.Cards.Add(playerBCard);
                playerB.Deck.Cards.Remove(playerBCard);
            }

            // If B wins, transfer A's card to him
            if (playerB.Deck.Cards.Contains(winnerCard))
            {
                _battleLog.Add($"{playerB.Username} has won Round {counter}!");
                playerB.Deck.Cards.Add(playerACard);
                playerA.Deck.Cards.Remove(playerACard);
            }
        }


        private void ProcessBattleOutcome(User playerA, User playerB, int counter)
        {
            _battleLog.Add($">>> Result <<<");
            if (playerA.Deck.Cards.Count > playerB.Deck.Cards.Count) // A wins
            {
                _battleLog.Add($"{playerA.Username} defeated {playerB.Username} in {counter - 1} rounds. Well done!");
                _userService.UpdateUserStats(playerA, playerB, false);
            }
            else if (playerB.Deck.Cards.Count > playerA.Deck.Cards.Count) // B wins
            {
                _battleLog.Add($"{playerB.Username} defeated {playerA.Username} in {counter - 1} rounds. Well done!");
                _userService.UpdateUserStats(playerB, playerA, false);
            }
            else // Draw
            {
                _battleLog.Add($"None of the players managed to win within 100 rounds. It's a tie!");
                _userService.UpdateUserStats(playerA, playerB, true);
            }
        }


        /// <summary>
        /// performs one round of combat between two playing cards
        /// </summary>
        /// <param name="cardA"></param>
        /// <param name="cardB"></param>
        /// <returns>
        /// <para>returns the card that won the round</para>
        /// <para>returns null on draw</para>
        /// </returns>
        private Card? FightOneRound(Card cardA, Card cardB)
        {
            // Check if specialties apply to battle
            Card? winner = BattleWithSpecialties(cardA, cardB);

            if (winner != null)
            {
                return winner;
            }

            // Regular battle
            cardA.TemporaryDamage = cardA.Damage;
            cardB.TemporaryDamage = cardB.Damage;

            if (cardA is SpellCard || cardB is SpellCard)
            {
                // at least one of the cards is a spell card -> element type has an effect on the damage calculation of this single round
                ApplyElementTypes(cardA, cardB);
            }
            else
            {
                // Pure Monster Fight (both cards are monsters) -> fights are not affected by the element type
                _battleLog.Add($"Both players have played monster cards -> Element types have no effect in this round!");
            }

            return CardFight(cardA, cardB);
        }


        /// <summary>
        /// modifies the cards' TemporaryDamage property depending on the Element Type Interactions
        /// </summary>
        /// <param name="cardA"></param>
        /// <param name="cardB"></param>
        private void ApplyElementTypes(Card cardA, Card cardB)
        {
            // water is effective against fire, so damage is doubled
            // fire is not effective against water, so damage is halved
            if (cardA.ElementType == ElementType.Water && cardB.ElementType == ElementType.Fire)
            {
                cardA.TemporaryDamage *= 2;
                cardB.TemporaryDamage /= 2;

                _battleLog.Add($"Water is very effective against fire, thus the damage of card {cardA.Name} is doubled to {cardA.TemporaryDamage}!");
                _battleLog.Add($"Fire is not effective against water, thus the damage of card {cardB.Name} is halved to {cardB.TemporaryDamage}!");
            }

            if (cardB.ElementType == ElementType.Water && cardA.ElementType == ElementType.Fire)
            {
                cardB.TemporaryDamage *= 2;
                cardA.TemporaryDamage /= 2;

                _battleLog.Add($"Water is very effective against fire, thus the damage of card {cardB.Name} is doubled to {cardB.TemporaryDamage}!");
                _battleLog.Add($"Fire is not effective against water, thus the damage of card {cardA.Name} is halved to {cardA.TemporaryDamage}!");
            }

            // fire is effective against normal, so damage is doubled
            // normal is not effective against fire, so damage is halved
            if (cardA.ElementType == ElementType.Fire && cardB.ElementType == ElementType.Normal)
            {
                cardA.TemporaryDamage *= 2;
                cardB.TemporaryDamage /= 2;

                _battleLog.Add($"Fire is very effective against normal, thus the damage of card {cardA.Name} is doubled to {cardA.TemporaryDamage}!");
                _battleLog.Add($"Normal is not effective against fire, thus the damage of card {cardB.Name} is halved to {cardB.TemporaryDamage}!");
            }

            if (cardB.ElementType == ElementType.Fire && cardA.ElementType == ElementType.Normal)
            {
                cardB.TemporaryDamage *= 2;
                cardA.TemporaryDamage /= 2;

                _battleLog.Add($"Fire is very effective against normal, thus the damage of card {cardB.Name} is doubled to {cardB.TemporaryDamage}!");
                _battleLog.Add($"Normal is not effective against fire, thus the damage of card {cardA.Name} is halved to {cardA.TemporaryDamage}!");
            }

            // normal is effective against water, so damage is doubled
            // water is not effective against normal, so damage is halved
            if (cardA.ElementType == ElementType.Normal && cardB.ElementType == ElementType.Water)
            {
                cardA.TemporaryDamage *= 2;
                cardB.TemporaryDamage /= 2;

                _battleLog.Add($"Normal is very effective against water, thus the damage of card {cardA.Name} is doubled to {cardA.TemporaryDamage}!");
                _battleLog.Add($"Water is not effective against normal, thus the damage of card {cardB.Name} is halved to {cardB.TemporaryDamage}!");
            }

            if (cardB.ElementType == ElementType.Normal && cardA.ElementType == ElementType.Water)
            {
                cardB.TemporaryDamage *= 2;
                cardA.TemporaryDamage /= 2;

                _battleLog.Add($"Normal is very effective against water, thus the damage of card {cardB.Name} is doubled to {cardB.TemporaryDamage}!");
                _battleLog.Add($"Water is not effective against normal, thus the damage of card {cardA.Name} is halved to {cardA.TemporaryDamage}!");
            }
        }


        /// <summary>
        /// compares the damage values of both cards and returns the winner
        /// </summary>
        /// <param name="cardA"></param>
        /// <param name="cardB"></param>
        /// <returns>
        /// <para>the winning card (card that has higher damage)</para>
        /// <para>null if it's a tie (both cards have the same damage)</para>
        /// </returns>
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

            return null;
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
            int r = Rnd.Next(deck.Cards.Count);
            return deck.Cards[r];
        }


        /// <summary>
        /// implements the Battle Logic for Special Cases
        /// </summary>
        /// <param name="cardA"></param>
        /// <param name="cardB"></param>
        /// <returns>
        ///<para>the winner's card if a special case applied to this fight</para>
        ///<para>null if no special case applied to this fight</para>
        /// </returns>
        private Card? BattleWithSpecialties(Card cardA, Card cardB)
        {
            // Goblins are too afraid of Dragons to attack
            if (cardA.Name.Contains("Goblin") && cardB.Name.Contains("Dragon"))
            {
                _battleLog.Add("The goblin is too afraid of the dragon and refuses to attack!");
                return cardB;
            }

            if (cardB.Name.Contains("Goblin") && cardA.Name.Contains("Dragon"))
            {
                _battleLog.Add("The goblin is too afraid of the dragon and refuses to attack!");
                return cardA;
            }

            // Wizards can control Orks so they are not able to damage them.
            if (cardA.Name.Contains("Wizard") && cardB.Name.Contains("Ork"))
            {
                _battleLog.Add("The wizard is able to control the ork and has thus won instantly!");
                return cardA;
            }

            if (cardB.Name.Contains("Wizard") && cardA.Name.Contains("Ork"))
            {
                _battleLog.Add("The wizard is able to control the ork and has thus won instantly!");
                return cardB;
            }

            // The armor of Knights is so heavy that WaterSpells make them drown them instantly.
            if (cardA.Name.Contains("Knight") && cardB.Name.Contains("WaterSpell"))
            {
                _battleLog.Add("The knight's heavy armour pulls him down and he drowns under the water spell!");
                return cardB;
            }

            if (cardB.Name.Contains("Knight") && cardA.Name.Contains("WaterSpell"))
            {
                _battleLog.Add("The knight's heavy armour pulls him down and he drowns under the water spell!");
                return cardA;
            }

            // The Kraken is immune against spells.
            if (cardA.Name.Contains("Kraken") && cardB.Name.Contains("Spell"))
            {
                _battleLog.Add("The kraken is unimpressed as it is immune to spells. It defeats the card with ease!");
                return cardA;
            }

            if (cardB.Name.Contains("Kraken") && cardA.Name.Contains("Spell"))
            {
                _battleLog.Add("The kraken is unimpressed as it is immune to spells. It defeats the card with ease!");
                return cardB;
            }

            // The FireElves know Dragons since they were little and can evade their attacks.
            if (cardA.Name.Contains("FireElf") && cardB.Name.Contains("Dragon"))
            {
                _battleLog.Add("The fire elf knows the dragon since they were kids and thus knows all his moves. The fire elf wins easily!");
                return cardA;
            }

            if (cardB.Name.Contains("FireElf") && cardA.Name.Contains("Dragon"))
            {
                _battleLog.Add("The fire elf knows the dragon since they were kids and thus knows all his moves. The fire elf wins easily!");
                return cardB;
            }

            return null;
        }
    }
}
