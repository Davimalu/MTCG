using MTCG.Models;

namespace MTCG.Interfaces;

public interface IUserService
{
    /// <summary>
    /// saves a new user to the database or - if the user already exists - updates his information
    /// </summary>
    /// <param name="user">user object | Stack and Deck are optional</param>
    /// <returns>ID of the newly created or updated database entry</returns>
    int SaveUserToDatabase(User user);

    /// <summary>
    /// retrieve a user from the database using his unique username
    /// </summary>
    /// <param name="username">the username of the user</param>
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

    User AddStackToUser(User user);
    User AddDeckToUser(User user);

    /// <summary>
    /// returns a list of usernames of all users currently registered to the game
    /// </summary>
    /// <returns>
    /// <para>A list of strings containing the usernames (unique) of each user</para>
    /// </returns>
    List<string> GetListOfUsers();

    string UserToJson(User user);
    void UpdateUserStats(User winner, User loser, bool tie);
}