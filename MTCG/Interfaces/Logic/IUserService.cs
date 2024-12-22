using MTCG.Models;

namespace MTCG.Interfaces.Logic;

public interface IUserService
{
    /// <summary>
    /// saves a new user to the database or - if the user already exists - updates his information
    /// </summary>
    /// <param name="user">user object containing all information to be saved/updated | Stack and Deck are optional</param>
    /// <returns>ID of the newly created or updated database entry</returns>
    int? SaveUserToDatabase(User user);
    /// <summary>
    /// retrieve a user from the database using his unique username
    /// </summary>
    /// <param name="username">username of the user</param>
    /// <returns>
    /// <para>user object containing all information + deck and stack on success</para>
    /// <para>null if there is no user with that username or an error occured</para>
    ///</returns>
    User? GetUserByName(string username);
    /// <summary>
    /// retrieve a user from the database using his authentication token
    /// </summary>
    /// <param name="token">the authentication token in format "xxx-mtcgToken"</param>
    /// <returns>
    /// <para>user object containing all information + deck and stack on success</para>
    /// <para>null if there is no user with that token or an error occured</para>
    /// </returns>
    User? GetUserByToken(string token);
    /// <summary>
    /// retrieve a user from the database using his unique Id
    /// </summary>
    /// <param name="userId">the id of the user</param>
    /// <returns>
    /// <para>user object containing all information + deck and stack on success</para>
    /// <para>null if there is no user with that Id or an error occured</para>
    ///</returns>
    User? GetUserById(int userId);
    /// <summary>
    /// retrieves the stack of the user from the database and adds it to the User object
    /// </summary>
    /// <param name="user">the user whose stack is to be retrieved</param>
    /// <returns>
    /// <para>User object containing his stack on success</para>
    /// <para>Unchanged user object in the event of an error or if the user's stack is empty</para>
    /// </returns>
    User AddStackToUser(User user);
    /// <summary>
    /// retrieves the deck of the user from the database and adds it to the User object
    /// </summary>
    /// <param name="user">the user whose deck is to be retrieved</param>
    /// <returns>
    /// <para>User object containing his deck on success</para>
    /// <para>Unchanged user object in the event of an error or if the user's deck is empty</para>
    /// </returns>
    User AddDeckToUser(User user);
    /// <summary>
    /// returns a list of usernames of all users currently registered to the game
    /// </summary>
    List<string> GetListOfUsers();
    /// <summary>
    /// converts some properties of a user object into JSON
    /// </summary>
    /// <param name="user"></param>
    /// <returns>JSON Representation of a User object containing all information intended for the end user</returns>
    string UserToJson(User user);
    /// <summary>
    /// updates the stats of users who have participated in a battle and writes the changes to the database
    /// </summary>
    /// <param name="winner">the user who won the battle</param>
    /// <param name="loser">the user who lost the battle</param>
    /// <param name="tie">if true, a tie is added instead to both the "winner" and the "loser"</param>
    void UpdateUserStats(User winner, User loser, bool tie);
}