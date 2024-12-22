using MTCG.Models;

namespace MTCG.Interfaces.Repository;

public interface ITradeRepository
{

    /// <summary>
    /// adds a new trade offer to the database
    /// </summary>
    /// <param name="offer">TradeOffer object containing all information that should be saved in the database</param>
    /// <returns>
    /// <para>tradeId (primary key) of the newly created database entry on success</para>
    /// <para>null on error</para>
    /// </returns>
    int? AddTradeOfferToDatabase(TradeOffer offer);
    /// <summary>
    /// removes a trade offer from the database
    /// </summary>
    /// <param name="offer">TradeOffer to be removed, represented as a TradeOffer object; MUST contain the tradeId</param>
    /// <returns>
    /// <para>true if the trade offer was successfully removed from the database</para>
    /// <para>null on error or invalid input (e.g. missing tradeId)</para>
    /// </returns>
    bool RemoveTradeOfferFromDatabase(TradeOffer offer);
    /// <summary>
    /// returns a list of all currently active trade offers stored in the database
    /// </summary>
    /// <returns>List of TradeOffer objects containing all the information stored in the `tradeDeals` table about them, the associated user and card</returns>
    List<TradeOffer> GetAllTradeOffers();
    /// <summary>
    /// retrieves a trade offer identified using its id
    /// </summary>
    /// <param name="tradeId">the id of the trade offer to retrieve</param>
    /// <returns>TradeOffer object containing all the information stored in the `tradeDeals` table about it, the associated user and card</returns>
    TradeOffer? GetTradeOfferById(int tradeId);
    /// <summary>
    /// retrieves a trade offer identified by the cardId of the card offered in that trade Offer
    /// </summary>
    /// <param name="cardIdToLookup">the id of the card offered in that trade Offer</param>
    /// <returns>TradeOffer object containing all the information stored in the `tradeDeals` table about it, the associated user and card</returns>
    TradeOffer? GetTradeDealByCardId(string cardIdToLookup);
    /// <summary>
    /// changes the status of a trade offer from active to inactive
    /// </summary>
    /// <param name="tradeIdToUpdate">tradeId (primary key) of the trade offer whose status is to be updated</param>
    /// <returns>
    /// <para>true on success</para>
    /// <para>false on error</para>
    /// </returns>
    bool SetTradeOfferInactive(int tradeIdToUpdate);
}