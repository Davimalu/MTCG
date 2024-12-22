using MTCG.Models;

namespace MTCG.Interfaces.Repository;

public interface IDeckRepository
{
    /// <summary>
    /// saves the deck of a user to the database
    /// </summary>
    /// <param name="user">user object containing his deck object</param>
    /// <returns>
    /// <para>true if the users deck was successfully stored in the database</para>
    /// <para>false if the user was not yet added to the database, the deck was empty or an error occured</para>
    /// </returns>
    bool SaveDeckOfUser(User user);

    /// <summary>
    /// retrieves all cards of a user's deck
    /// </summary>
    /// <param name="user"></param>
    /// <returns>a list of all the cardIds of the cards from the user's deck</returns>
    List<string>? GetCardIdsOfUserDeck(User user);

    /// <summary>
    /// delete the deck of a user form the database
    /// </summary>
    /// <param name="user">user object, must contain at least the userId</param>
    /// <returns>
    /// <para>true on success</para>
    /// <para>false if user or his deck were not yet added to database or on error</para>
    /// </returns>
    bool ClearUserDeck(User user);
}