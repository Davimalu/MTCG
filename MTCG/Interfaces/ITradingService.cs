using MTCG.Models;

namespace MTCG.Interfaces;

public interface ITradingService
{
    TradeDeal? CreateTradeOffer(User user, Card card, bool requestedMonsterCard, float requestedDamage);
    bool RemoveTradeOfferByCardId(string cardId);
    TradeDeal? GetTradeOfferByCardId(string cardId);
    TradeDeal GetTradeOfferById(int tradeId);
    List<TradeDeal> GetTradeOffers();
    bool TryToTrade(TradeDeal newDeal);
}