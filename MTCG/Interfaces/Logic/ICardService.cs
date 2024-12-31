using MTCG.Models;
using MTCG.Models.Cards;

namespace MTCG.Interfaces.Logic;

public interface ICardService
{

    /// <summary>
    /// retrieves a card by its ID
    /// </summary>
    /// <param name="cardId">string representing the cardID</param>
    /// <returns>
    /// <para>instance of a card Object containing all information stored in the database</para>
    /// <para>null if there is no card with that ID</para>
    /// </returns>
    Card? GetCardById(string cardId);
    /// <summary>
    /// saves a new card to the database
    /// </summary>
    /// <param name="card">instance of a card object containing all information to be saved to the database</param>
    /// <returns>
    /// <para>true on success</para>
    /// <para>false on error</para>
    /// </returns>
    bool SaveCardToDatabase(Card card);
    /// <summary>
    /// deletes a card, identified by its cardId
    /// </summary>
    /// <param name="card">Monster- or SpellCard containing the cardId by which the card was saved to the database</param>
    /// <returns>
    /// <para>True if card was successfully deleted</para>
    /// <para>False on error or if card was never saved</para>
    /// </returns>
    bool DeleteCardFromDatabase(Card card);
    bool UserOwnsCard(User user, Card card);
    bool UserHasCardInDeck(User user, Card card);
    /// <summary>
    /// serializes a given list of cards into a human-readable JSON representation
    /// </summary>
    /// <param name="cards">Stack, Deck or other object that contains an array of cards named `Cards`</param>
    /// <returns></returns>
    string SerializeCardsToJson(IEnumerable<Card> cards);
}