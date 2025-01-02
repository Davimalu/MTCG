using MTCG.HTTP;
using MTCG.Interfaces.HTTP;
using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MTCG.Endpoints
{
    public class DeckEndpoint : IHttpEndpoint
    {
        private readonly ICardService _cardService = CardService.Instance;
        private readonly IUserService _userService = UserService.Instance;
        private readonly IDeckService _deckService = DeckService.Instance;
        private readonly IHttpHeaderService _httpHeaderService = new HttpHeaderService();

        private readonly IEventService _eventService = new EventService();

        public DeckEndpoint()
        {

        }

        #region DependencyInjection
        public DeckEndpoint(ICardService cardService, IUserService userService, IDeckService deckService,
            IHttpHeaderService httpHeaderService, IEventService eventService)
        {
            _cardService = cardService;
            _userService = userService;
            _deckService = deckService;
            _httpHeaderService = httpHeaderService;
            _eventService = eventService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HttpHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _httpHeaderService.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }


            switch (headers.Method)
            {
                // List deck of user
                case "GET":
                    return HandleGetUserDeck(headers, user);
                // Update deck of user
                case "PUT":
                    return HandleUpdateUserDeck(body, user);
                default:
                    return (405, JsonSerializer.Serialize("Method Not Allowed"));
            }
        }


        private (int, string?) HandleGetUserDeck(HttpHeader headers, User user)
        {
            // Check for query Parameters
            Dictionary<string, string> queryParameters = _httpHeaderService.GetQueryParameters(headers);

            // https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.dictionary-2.trygetvalue?view=net-8.0
            if (queryParameters.TryGetValue("format", out string? format))
            {
                // Query Parameter format present
                if (format == "plain")
                {
                    _eventService.LogEvent(EventType.Highlight, $"Retrieved deck of user {user.Username} in plaintext", null);
                    return (200, _deckService.SerializeDeckToPlaintext(user.Deck));
                }
                else
                {
                    return (400, JsonSerializer.Serialize("Invalid query parameter"));
                }
            }
            else // No Query Parameters
            {
                _eventService.LogEvent(EventType.Highlight, $"Retrieved deck of user {user.Username} in JSON", null);
                return (200, _cardService.SerializeCardsToJson(user.Deck.Cards));
            }
        }


        private (int, string?) HandleUpdateUserDeck(string? body, User user)
        {
            if (body == null)
            {
                return (400, JsonSerializer.Serialize("Empty request body"));
            }

            List<Card>? configuredDeck = ProcessDeckConfiguration(user, body);
            if (configuredDeck == null)
            {
                return (400, JsonSerializer.Serialize("Invalid deck configuration provided"));
            }

            user.Deck.Cards = configuredDeck;

            // Save changes to database
            _userService.SaveUserToDatabase(user);

            _eventService.LogEvent(EventType.Highlight, $"Deck of user {user.Username} updated", null);

            var response = new JsonObject()
            {
                ["message"] = "Deck updated",
                ["Username"] = user.Username,
                ["Deck"] = JsonNode.Parse(_cardService.SerializeCardsToJson(user.Deck.Cards))!
            };

            return (200, response.ToJsonString());
        }


        /// <summary>
        /// parses the JSON deck configuration of a user and checks if it's valid
        /// </summary>
        /// <param name="user">user object of the user whose deck should be configured</param>
        /// <param name="body">the body of the request</param>
        /// <returns>
        /// <para>returns a list of cards representing the deck configuration that was chosen by the user on success</para>
        /// <para>returns null if the request format was invalid or the user doesn't own the cards</para>
        /// </returns>
        private List<Card>? ProcessDeckConfiguration(User user, string body)
        {
            // Check if a valid request body is present
            List<string>? cardIds;
            try
            {
                cardIds = JsonSerializer.Deserialize<List<string>>(body);
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Warning, $"Error parsing deck configuration of user {user.Username}", ex);
                return null;
            }

            // Check if exactly 4 cards were provided
            if (cardIds == null || cardIds.Count != 4)
            {
                _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to update his deck with an invalid deck configuration", null);
                return null;
            }

            List<Card> configuredDeck = new List<Card>(4);

            // For each card in the configuration provided...
            foreach (string cardId in cardIds)
            {
                Card? card = _cardService.GetCardById(cardId);

                // Card doesn't exist
                if (card == null)
                {
                    _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to configure his deck with a card that doesn't exist", null);
                    return null;
                }

                // Check if user owns the card (= user has this card in his stack)
                if (user.Stack.Cards.All(stackCard => stackCard.Id != card.Id)) // LINQ Expression
                {
                    _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to configure his deck with a card he doesn't own", null);
                    return null;
                }

                configuredDeck.Add(card);
            }

            return configuredDeck;
        }
    }
}
