using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public List<TradeDeal> GetTradeDeals()
        {
            return _tradeRepository.GetAllTradeDeals();
        }
    }
}
