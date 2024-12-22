using MTCG.Models.Cards;

namespace MTCG.Repository;

public interface ICardRepository
{
    /// <summary>
    /// saves a new card to the database
    /// </summary>
    /// <param name="card"></param>
    /// <returns>
    /// <para>true if card was successfully added to database</para>
    /// <para>false if card couldn't be added to database</para>
    /// </returns>>
    bool AddCardToDatabase(Card card);

    /// <summary>
    /// Retrieves a card - identified by its cardId - from the database
    /// </summary>
    /// <param name="cardId"></param>
    /// <returns>
    /// <para>Monster- or Spellcard object</para>
    /// <para>null if there is no card in the database with the specified Id</para>
    /// </returns>
    Card? GetCardById(string cardId);
}