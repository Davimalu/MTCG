using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Interfaces.Logic;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class TradingService : ITradingService
    {
        #region Singleton
        private static TradingService? _instance;

        public static TradingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TradingService();
                }

                return _instance;
            }
        }
        #endregion

        private readonly TradeRepository _tradeRepository = TradeRepository.Instance;
        private readonly UserService _userService = UserService.Instance;
        private readonly CardRepository _cardRepository = CardRepository.Instance;

        public TradeDeal? CreateTradeOffer(User user, Card card, bool requestedMonsterCard, float requestedDamage)
        {
            TradeDeal deal = new TradeDeal()
            {
                Id = null,
                User = user,
                Card = card,
                RequestedMonster = requestedMonsterCard,
                RequestedDamage = requestedDamage
            };

            int? tradeId = _tradeRepository.AddTradeDeal(deal);

            if (tradeId != null)
            {
                deal.Id = tradeId;

                // Remove card from stack of user
                var cardToRemove = user.Stack.Cards.FirstOrDefault(item => item.Id == card.Id);
                if (cardToRemove != null)
                {
                    user.Stack.Cards.Remove(cardToRemove);
                }

                _userService.SaveUserToDatabase(user);

                // Now that the offer was created, see if there is another trade offer that is compatible with this one
                TryToTrade(deal);

                return deal;
            }

            return null;
        }

        public bool RemoveTradeOfferByCardId(string cardId)
        {
            TradeDeal dealToRemove = GetTradeOfferByCardId(cardId);

            if (_tradeRepository.RemoveTradeDeal(dealToRemove))
            {
                // If the trade offer was successfully deleted, re-add card to stack of user
                dealToRemove.User.Stack.Cards.Add(dealToRemove.Card);
                _userService.SaveUserToDatabase(dealToRemove.User);
                return true;
            }

            return false;
        }

        public TradeDeal? GetTradeOfferByCardId(string cardId)
        {
            TradeDeal deal = _tradeRepository.GetTradeDealByCardId(cardId);

            if (deal != null)
            {
                // Populate user and card field
                deal.User = _userService.GetUserById(deal.User.Id);
                deal.Card = _cardRepository.GetCardById(deal.Card.Id);
            }

            return deal;
        }

        public TradeDeal GetTradeOfferById(int tradeId)
        {
            TradeDeal deal = _tradeRepository.GetTradeDealById(tradeId);

            // Populate user and card field
            deal.User = _userService.GetUserById(deal.User.Id);
            deal.Card = _cardRepository.GetCardById(deal.Card.Id);

            return deal;
        }

        public List<TradeDeal> GetTradeOffers()
        {
            List<TradeDeal> deals = _tradeRepository.GetAllTradeDeals();

            // Populate user and card fields of all results
            foreach (TradeDeal deal in deals)
            {
                deal.User = _userService.GetUserById(deal.User.Id);
                deal.Card = _cardRepository.GetCardById(deal.Card.Id);
            }

            return deals;
        }

        public bool TryToTrade(TradeDeal newDeal)
        {
            // Ensure that no user information is changed while this function is executed - otherwise the User objects might contain outdated information by the time SaveUserToDatabase() is called
            lock (ThreadSync.UserLock)
            {
                List<TradeDeal> allOffers = GetTradeOffers();

                // No other offers exist -> cannot trade
                if (allOffers.Count < 2)
                {
                    return false;
                }

                foreach (TradeDeal deal in allOffers)
                {
                    if (deal.User.Username == newDeal.User.Username || deal.Id == null ||newDeal.Id == null)
                    {
                        continue;
                    }

                    // Check if the card of the new offer is compatible with the requirements of existing offers
                    if ((newDeal.Card is MonsterCard && deal.RequestedMonster == true) ||
                        (!(newDeal.Card is MonsterCard) && deal.RequestedMonster == false))
                    {
                        if (newDeal.Card.Damage >= deal.RequestedDamage)
                        {
                            // The other offer would accept this card
                            // Now check if the requirements of this offer would accept the card of the other offer
                            if ((deal.Card is MonsterCard && newDeal.RequestedMonster == true) ||
                                !(deal.Card is MonsterCard) && newDeal.RequestedMonster == false)
                            {
                                if (deal.Card.Damage >= newDeal.RequestedDamage)
                                {
                                    // Both trade offers are compatible, commence trade!
                                    deal.User.Stack.Cards.Add(newDeal.Card);
                                    newDeal.User.Stack.Cards.Add(deal.Card);

                                    _userService.SaveUserToDatabase(deal.User);
                                    _userService.SaveUserToDatabase(newDeal.User);

                                    _tradeRepository.SetTradeOfferInactive((int)deal.Id);
                                    _tradeRepository.SetTradeOfferInactive((int)newDeal.Id);

                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }
    }
}
