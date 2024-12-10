using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MTCG.HTTP;
using MTCG.Interfaces;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Endpoints
{
    public class TradingsEndpoint : IHttpEndpoint
    {
        private readonly UserService _userService = UserService.Instance;
        private readonly TradingService _tradingService = TradingService.Instance;
        private readonly CardRepository _cardRepository = CardRepository.Instance;
        private readonly CardService _cardService = CardService.Instance;

        public (int, string?) HandleRequest(TcpClient client, HTTPHeader headers, string? body)
        {
            // Check if user is authorized
            string token = HeaderHelper.GetTokenFromHeader(headers)!;
            User? user = _userService.GetUserByToken(token);

            if (user == null)
            {
                return (401, JsonSerializer.Serialize("User not authorized"));
            }

            // List all active trade offers
            if (headers.Method == "GET")
            {
                List<TradeDeal> deals = _tradingService.GetTradeDeals();

                List<TradeOffer> offers = new List<TradeOffer>();
                foreach (TradeDeal deal in deals)
                {
                    offers.Add(new TradeOffer()
                    {
                        Username = deal.User.Username,
                        Card = new
                        {
                            Id = deal.Card.Id,
                            Name = deal.Card.Name,
                            Damage = deal.Card.Damage,
                            Type = deal.Card is MonsterCard ? "Monster" : "Spell"
                        },
                        RequestedType = deal.RequestedMonster == true ? "Monster" : "Spell",
                        RequestedDamage = deal.RequestedDamage
                    });
                }

                string json = JsonSerializer.Serialize(offers);
                return (200, json);
            }

            // Create a new trade offer
            if (headers.Method == "POST")
            {
                string? cardId, type;
                float minimumDamage = -1;

                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(body))
                    {
                        JsonElement root = doc.RootElement;

                        cardId = root.GetProperty("CardToTrade").GetString();
                        type = root.GetProperty("Type").GetString();
                        minimumDamage = root.GetProperty("MinimumDamage").GetSingle();
                    }
                }
                catch (Exception ex)
                {
                    return (400, JsonSerializer.Serialize("Invalid request body"));
                }

                if (cardId == null || type == null || minimumDamage < 0)
                {
                    return (400, JsonSerializer.Serialize("Trade offer incomplete"));
                }

                Card? cardToTrade = _cardRepository.GetCardById(cardId);

                if (cardToTrade == null)
                {
                    return (400, JsonSerializer.Serialize("Selected card doesn't exist"));
                }

                if (!_cardService.UserOwnsCard(user, cardToTrade))
                {
                    return (403, JsonSerializer.Serialize("You cannot trade a card you don't own"));
                }

                if (_cardService.UserHasCardInDeck(user, cardToTrade))
                {
                    return (409, JsonSerializer.Serialize("You cannot trade a cade that is currently in your deck"));
                }

                TradeDeal? deal = _tradingService.CreateTradeDeal(user, cardToTrade, type == "monster" ? true : false, minimumDamage);

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

                return (201, json);
            }

            // Delete trade deal
            if (headers.Method == "DELETE")
            {
                // Get cardId of the trade the user wants to remove

                // Find the index of the last slash ('/')
                int index = headers.Path.LastIndexOf('/') + 1;
                // Extract the substring starting after the last slash
                string cardIdOfTradeToRemove = headers.Path.Substring(index);

                TradeDeal? dealToRemove = _tradingService.GetTradeDealByCardId(cardIdOfTradeToRemove);

                if (dealToRemove == null)
                {
                    return (400, JsonSerializer.Serialize("There is no trade deal for this card"));
                }

                if (dealToRemove.User.Username != user.Username)
                {
                    return (403, JsonSerializer.Serialize("You cannot delete other peoples' trade offers!"));
                }

                if (_tradingService.RemoveTradeDealByCardId(cardIdOfTradeToRemove))
                {
                    return (200, JsonSerializer.Serialize("Trade offer removed"));
                }
                else
                {
                    return (500, JsonSerializer.Serialize("Unknown error removing trade offer"));
                }
            }

            return (405, "Method Not Allowed");
        }

        internal class TradeOffer {
            public string Username { get; set; }
            public Object Card { get; set; }
            public string RequestedType { get; set; } = string.Empty;
            public float RequestedDamage { get; set; }
        }
    }
}
