using MTCG.Models;

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
    bool UserOwnsCard(User user, Card card);
    bool UserHasCardInDeck(User user, Card card);
}