using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Interfaces;
using MTCG.Models;
using MTCG.Repository;

namespace MTCG.Logic
{
    public class TradingService
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

        public TradeDeal? CreateTradeDeal(User user, Card card, bool requestedMonsterCard, float requestedDamage)
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
                return deal;
            }

            return null;
        }

        public bool RemoveTradeDeal()
        {
            return false;
        }

        public TradeDeal GetTradeDealById(int tradeId)
        {
            TradeDeal deal = _tradeRepository.GetTradeDealById(tradeId);

            // Populate user and card field
            deal.User = _userService.GetUserById(deal.User.Id);
            deal.Card = _cardRepository.GetCardById(deal.Card.Id);

            return deal;
        }

        public List<TradeDeal> GetTradeDeals()
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
    }
}
