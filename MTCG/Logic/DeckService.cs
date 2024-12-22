using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Models.Enums;

namespace MTCG.Logic
{
    public class DeckService : IDeckService
    {
        #region Singleton
        private static DeckService? _instance;

        public static DeckService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeckService();
                }

                return _instance;
            }
        }
        #endregion
        #region DependencyInjection
        public DeckService(IEventService eventService)
        {
            _eventService = eventService;
        }
        #endregion

        public DeckService() { }

        private readonly IEventService _eventService = new EventService();


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
                _eventService.LogEvent(EventType.Warning, $"Couldn't add card with ID {card.Id} to deck", ex);
                return false;
            }
        }


        public void RemoveCardFromUserDeck(Card cardToRemove, Deck deck)
        {
            deck.Cards.RemoveAll(card => card.Id == cardToRemove.Id);
        }


        public string SerializeDeckToPlaintext(Deck deck)
        {
            string returnString = String.Empty;

            foreach (Card card in deck.Cards)
            {
                returnString +=
                    $"ID: {card.Id}, Name: {card.Name}, Damage: {card.Damage}, Card Type: {(card is MonsterCard ? "Monster Card" : "Spell Card")}, Element Type: {card.ElementType.ToString()}\n";
                // use `is` operator to determine type of card: https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/is
            }

            return returnString;
        }
    }
}
