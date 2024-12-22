using MTCG.Models.Cards;

namespace MTCG.Interfaces.Repository;

public interface ICardRepository
{
    /// <summary>
    /// saves a new card to the database
    /// </summary>
    /// <param name="card">card object containing all information that is supposed to be added to the database</param>
    /// <returns>
    /// <para>true if card was successfully added to database</para>
    /// <para>false if card couldn't be added to database</para>
    /// </returns>>
    bool AddCardToDatabase(Card card);
    /// <summary>
    /// Retrieves a card - identified by its cardId - from the database
    /// </summary>
    /// <param name="cardId">the ID of the card to be retrieved</param>
    /// <returns>
    /// <para>Monster- or SpellCard object containing all information stored in the database about that card</para>
    /// <para>null on error if there is no card in the database with the specified Id</para>
    /// </returns>
    Card? GetCardById(string cardId);
}