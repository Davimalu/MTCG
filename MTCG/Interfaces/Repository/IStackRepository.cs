using MTCG.Models;

namespace MTCG.Interfaces.Repository;

public interface IStackRepository
{
    /// <summary>
    /// saves the Stack of a user to the database
    /// </summary>
    /// <param name="user">user object containing the Stack object to be stored in the database</param>
    /// <returns>
    /// <para>true if the users Stack was successfully stored in the database</para>
    /// <para>false if the user was not yet added to the database, the Stack was empty or an error occured</para>
    /// </returns>
    bool SaveStackOfUser(User user);
    /// <summary>
    /// retrieves all cards of a user's stack
    /// </summary>
    /// <param name="user"></param>
    /// <returns>a list of all the cardIds of the cards from the user's stack</returns>
    List<string>? GetCardIdsOfUserStack(User user);
    /// <summary>
    /// delete the stack of a user form the database
    /// </summary>
    /// <param name="user">user object, must contain at least the userId</param>
    /// <returns>
    /// <para>true on success</para>
    /// <para>false if user or his stack were not yet added to database or on error</para>
    /// </returns>
    bool ClearUserStack(User user);
}