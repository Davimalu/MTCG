using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class CardService : ICardService
    {
        private readonly CardRepository _cardRepository = CardRepository.Instance;

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


        public Card? GetCardById(string cardId)
        {
            return _cardRepository.GetCardById(cardId);
        }


        public bool SaveCardToDatabase(Card card)
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

            return _cardRepository.AddCardToDatabase(cardToAdd);
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
    }
}
