using MTCG.Models;

namespace MTCG.Interfaces.Repository;

public interface ITradeRepository
{
    int? AddTradeOfferToDatabase(TradeOffer offer);
    bool RemoveTradeDeal(TradeOffer offer);
    List<TradeOffer> GetAllTradeDeals();
    TradeOffer? GetTradeDealById(int tradeId);
    TradeOffer? GetTradeDealByCardId(string cardIdToLookup);
    int SetTradeOfferInactive(int tradeIdToUpdate);
}