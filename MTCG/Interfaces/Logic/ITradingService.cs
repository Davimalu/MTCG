using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface ITradingService
{
    /// <summary>
    /// create a new trade deal using the specified parameters
    /// </summary>
    /// <param name="user">the user who wants to create a trade offer</param>
    /// <param name="card">the card the user wants to offer for trading</param>
    /// <param name="requestedMonsterCard">true if the user demands a MonsterCard in Return, false if he demands a SpellCard</param>
    /// <param name="requestedDamage">minimum amount of damage the user demands to accept the trade deal</param>
    /// <returns>
    /// <para>TradeOffer Object representing the created trade offer</para>
    /// <para>null on failure to create the trade offer or if some parameters were missing</para>
    /// </returns>
    TradeOffer? CreateTradeOffer(User user, Card card, bool requestedMonsterCard, float requestedDamage);
    /// <summary>
    /// remove a trade offer specified by the cardId of the card that was offered in the TradeOffer
    /// </summary>
    /// <param name="cardId">cardId of the card that was offered in the TradeOffer to remove</param>
    /// <returns>
    /// <para>true if the trade offer was successfully removed</para>
    /// <para>false if there was no trade offer for that card or on error</para>
    /// </returns>
    bool RemoveTradeOfferByCardId(string cardId);
    /// <summary>
    /// retrieve a trade offer specified by the cardId of the card that was offered in the TradeOffer
    /// </summary>
    /// <param name="cardId">cardId of the card that was offered in the TradeOffer</param>
    /// <returns>
    /// <para>a TradeOffer object containing all information about the trade offer on success</para>
    /// <para>null if there is no trade offer for that card or on error</para>
    /// </returns>
    TradeOffer? GetTradeOfferByCardId(string cardId);
    /// <summary>
    /// retrieve a trade offer specified by the ID (Primary Key) of the tradeOffer
    /// </summary>
    /// <param name="tradeId">the ID of the trade offer</param>
    /// <returns>
    /// <para>a TradeOffer object containing all information about the trade offer on success</para>
    /// <para>null if there is no trade offer with that ID or on error</para>
    /// </returns>
    TradeOffer? GetTradeOfferById(int tradeId);
    /// <summary>
    /// retrieves a list of all currently active trade offers
    /// </summary>
    /// <returns></returns>
    List<TradeOffer> GetAllActiveTradeOffers();
    /// <summary>
    /// checks whether a new trade offer is compatible with the requirements of an existing trade offer (and vice versa) and executes the trade if so
    /// </summary>
    /// <param name="newOffer">the newly added trade offer that should be checked against all already existing trade offers</param>
    /// <returns>
    /// <para>true if a trade took place (a compatible trade offer was found)</para>
    /// <para>false if no trade took place (no compatible trade offer was found)</para>
    /// </returns>
    bool TryToTrade(TradeOffer newOffer);
}