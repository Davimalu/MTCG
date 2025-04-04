﻿using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using MTCG.Repository;
using System.Text.Json;

namespace MTCG.Logic
{
    public class CardService : ICardService
    {
        private readonly ICardRepository _cardRepository = CardRepository.Instance;

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
        #region DependencyInjection
        public CardService(ICardRepository cardRepository)
        {
            _cardRepository = cardRepository;
        }
        #endregion
        public CardService() { }

        public Card? GetCardById(string cardId)
        {
            return _cardRepository.GetCardById(cardId);
        }


        public Card? SaveCardToDatabase(Card card)
        {
            // Determine the element type of the card
            if (Enum.TryParse(GetElementTypeFromName(card), out ElementType elementType))
            {
                card.ElementType = elementType;
            }

            // Determine the type (monster or spell card) of the card
            Card cardToAdd;
            if (GetCardTypeFromName(card) == "Monster")
            {
                cardToAdd = new MonsterCard(card);
            }
            else
            {
                cardToAdd = new SpellCard(card);
            }

            if (_cardRepository.AddCardToDatabase(cardToAdd))
            {
                return cardToAdd;
            }
            return null;
        }


        public bool DeleteCardFromDatabase(Card card)
        {
            return _cardRepository.DeleteCardFromDatabase(card);
        }


        public bool UserOwnsCard(User user, Card card)
        {
            // https://stackoverflow.com/questions/4651285/checking-if-a-list-of-objects-contains-a-property-with-a-specific-value
            return user.Stack.Cards.Any(cardsInStack => cardsInStack.Id == card.Id);
        }


        public bool UserHasCardInDeck(User user, Card card)
        {
            // https://stackoverflow.com/questions/4651285/checking-if-a-list-of-objects-contains-a-property-with-a-specific-value
            return user.Deck.Cards.Any(cardsInDeck => cardsInDeck.Id == card.Id);
        }


        /// <summary>
        /// determines whether the card is a Monster or a Spell Card from the name of the card
        /// </summary>
        /// <param name="card"></param>
        /// <returns>
        /// <para>"Spell" if the card is a Spell Card</para>
        /// <para>"Monster" if the card is a Monster Card</para>
        /// </returns>
        private string GetCardTypeFromName(Card card)
        {
            if (card.Name.Contains("Spell"))
            {
                return "Spell";
            }
            else
            {
                return "Monster";
            }
        }


        /// <summary>
        /// determines the element type of the card from the name of the card
        /// </summary>
        /// <param name="card"></param>
        /// <returns>
        /// <para>"Water" if the Element Type is Water</para>
        /// <para>"Fire" if the Element Type is Fire</para>
        /// <para>"Normal" if the Element Type is Normal</para>
        /// </returns>
        private string GetElementTypeFromName(Card card)
        {
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


        public string SerializeCardsToJson(IEnumerable<Card> cards)
        {
            List<FrontendCard> frontendCards = new List<FrontendCard>();

            foreach (Card card in cards)
            {
                frontendCards.Add(BackendCardToFrontendCard(card));
            }

            return JsonSerializer.Serialize(frontendCards);
        }


        /// <summary>
        /// converts a card from its representation in the backend to a more human-readable frontend representation
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        private FrontendCard BackendCardToFrontendCard(Card card)
        {
            FrontendCard newCard = new FrontendCard()
            {
                CardId = card.Id ?? "N/A",
                CardName = card.Name,
                Damage = card.Damage,
                CardType = card is MonsterCard ? "Monster Card" : "Spell Card",
                ElementType = card.ElementType.ToString()
            };

            return newCard;
        }
    }
}
