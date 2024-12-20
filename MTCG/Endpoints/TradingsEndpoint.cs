using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using MTCG.Repository;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.Endpoints
{
    public class TradingsEndpoint : IHttpEndpoint
    {
        private readonly IUserService _userService = UserService.Instance;
        private readonly ITradingService _tradingService = TradingService.Instance;
        private readonly ICardService _cardService = CardService.Instance;
        private readonly IHeaderHelper _headerHelper = new HeaderHelper();

        private readonly IEventService _eventService = new EventService();

        public TradingsEndpoint()
        {

        }

        #region DependencyInjection
        public TradingsEndpoint(IUserService userService, ITradingService tradingService, ICardService cardsService,
            IHeaderHelper headerHelper, IEventService eventService)
        {
            _userService = userService;
            _tradingService = tradingService;
            _cardService = cardsService;
            _headerHelper = headerHelper;
            _eventService = eventService;
        }
        #endregion

        public (int, string?) HandleRequest(TcpClient? client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = _headerHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }

            switch (headers.Method)
            {
                // List all active trade offers
                case "GET":
                    return HandleListTradeOffers();
                // Create a new trade offer
                case "POST":
                    return HandleCreateTradeOffer(headers, body, user);
                // Delete trade offer
                case "DELETE":
                    return HandleDeleteTradeOffer(headers, user);
                default:
                    return (405, "Method Not Allowed");
            }
        }

        private (int, string?) HandleListTradeOffers()
        {
            List<TradeDeal> tradeOffers = _tradingService.GetTradeOffers();

            List<TradeOffer> response = new List<TradeOffer>();
            foreach (TradeDeal offer in tradeOffers)
            {
                response.Add(new TradeOffer()
                {
                    Username = offer.User.Username,
                    Card = new
                    {
                        Id = offer.Card.Id,
                        Name = offer.Card.Name,
                        Damage = offer.Card.Damage,
                        Type = offer.Card is MonsterCard ? "Monster" : "Spell"
                    },
                    RequestedType = offer.RequestedMonster == true ? "Monster" : "Spell",
                    RequestedDamage = offer.RequestedDamage
                });
            }

            string json = JsonSerializer.Serialize(response);
            return (200, json);
        }

        private (int, string?) HandleCreateTradeOffer(HTTPHeader headers, string? body, User user)
        {
            string? cardId, type;
            float minimumDamage = -1;

            if (body == null)
            {
                return (400, JsonSerializer.Serialize("Empty request body"));
            }

            // Try to parse the request body
            try
            {
                using JsonDocument doc = JsonDocument.Parse(body);
                JsonElement root = doc.RootElement;

                cardId = root.GetProperty("CardToTrade").GetString();
                type = root.GetProperty("Type").GetString();
                minimumDamage = root.GetProperty("MinimumDamage").GetSingle();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Warning, $"Couldn't parse request body provided by User {user.Username}", ex);
                return (400, JsonSerializer.Serialize("Invalid request body"));
            }

            // Check if valid values were provided
            if (cardId == null || type == null || minimumDamage < 0)
            {
                _eventService.LogEvent(EventType.Warning, $"{user.Username} provided insufficient information to create a trade offer", null);
                return (400, JsonSerializer.Serialize("Trade offer incomplete"));
            }

            Card? cardToTrade = _cardService.GetCardById(cardId);

            if (cardToTrade == null)
            {
                _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to trade a card that doesn't exist", null);
                return (400, JsonSerializer.Serialize("Selected card doesn't exist"));
            }

            if (!_cardService.UserOwnsCard(user, cardToTrade))
            {
                _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to trade a card he/she doesn't own", null);
                return (403, JsonSerializer.Serialize("You cannot trade a card you don't own"));
            }

            if (_cardService.UserHasCardInDeck(user, cardToTrade))
            {
                _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to trade a card that is currently in his/her deck", null);
                return (409, JsonSerializer.Serialize("You cannot trade a cade that is currently in your deck"));
            }

            // If none of these checks failed, create the trade offer...
            TradeDeal? deal = _tradingService.CreateTradeOffer(user, cardToTrade, type == "monster", minimumDamage);

            if (deal == null)
            {
                return (500, JsonSerializer.Serialize("Unknown error creating trade deal"));
            }

            var response = new
            {
                message = "Trade offer created",
                Username = user.Username,
                Card = new
                {
                    Id = deal.Card.Id,
                    Name = deal.Card.Name,
                    Damage = deal.Card.Damage,
                    Type = deal.Card is MonsterCard ? "Monster" : "Spell"
                },
                RequestedType = deal.RequestedMonster == true ? "Monster" : "Spell",
                RequestedDamage = deal.RequestedDamage
            };
            string json = JsonSerializer.Serialize(response);

            _eventService.LogEvent(EventType.Highlight, $"User {user.Username} created a new trade offer for Card {cardToTrade.Name}", null);
            return (201, json);
        }

        private (int, string?) HandleDeleteTradeOffer(HTTPHeader headers, User user)
        {
            // Get cardId of the trade the user wants to remove

            // Find the index of the last slash ('/')
            int index = headers.Path.LastIndexOf('/') + 1;
            // Extract the substring starting after the last slash | /tradings/27051a20-8580-43ff-a473-e986b52f297a -> 27051a20-8580-43ff-a473-e986b52f297a
            string cardIdOfTradeToRemove = headers.Path.Substring(index);

            TradeDeal? dealToRemove = _tradingService.GetTradeOfferByCardId(cardIdOfTradeToRemove);

            if (dealToRemove == null)
            {
                _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to delete a trade offer that doesn't exist", null);
                return (400, JsonSerializer.Serialize("There is no trade offer for this card"));
            }

            if (dealToRemove.User.Username != user.Username)
            {
                _eventService.LogEvent(EventType.Warning, $"User {user.Username} tried to delete the trade offer of another person", null);
                return (403, JsonSerializer.Serialize("You cannot delete other peoples' trade offers!"));
            }

            // If none of these checks failed, remove the trade offer...
            if (_tradingService.RemoveTradeOfferByCardId(cardIdOfTradeToRemove))
            {
                _eventService.LogEvent(EventType.Highlight, $"User {user.Username} deleted one of his trade offers", null);
                return (200, JsonSerializer.Serialize("Trade offer removed"));
            }
            else
            {
                return (500, JsonSerializer.Serialize("Unknown error removing trade offer"));
            }
        }
    }
    internal class TradeOffer
    {
        public required string Username { get; set; }
        public required Object Card { get; set; }
        public string RequestedType { get; set; } = string.Empty;
        public float RequestedDamage { get; set; }
    }
}
