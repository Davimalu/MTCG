using MTCG.DAL;
using MTCG.Models;

namespace MTCG.Interfaces.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// saves a new user to the database
        /// </summary>
        /// <param name="user">user object containing all the data to be written to the database, Stack and Deck are optional/ignored</param>
        /// <returns>
        /// <para>userID (primary key) of the newly created database entry on success</para>
        /// <para>null on error</para>
        /// </returns>
        int? SaveUserToDatabase(User user);
        /// <summary>
        /// retrieve a user from the database using his name
        /// </summary>
        /// <param name="username">username (primary key) of the user to be retrieved from the database</param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that name or an error occured</para>
        /// </returns>
        User? GetUserByName(string username);
        /// <summary>
        /// update the information of an existing user in the users table; does NOT update the users stack or deck
        /// </summary>
        /// <param name="user">user object containing the updated user data, Stack and Deck are optional/ignored</param>
        /// <returns>
        /// <para>ID of the updated database entry on success</para>
        /// <para>null if the user has not yet been added to the database or on error</para>
        /// </returns>
        int? UpdateUser(User user);
        /// <summary>
        /// retrieve a user from the database using his authentication token
        /// </summary>
        /// <param name="token">the authentication token in format "xxx-mtcgToken"</param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that token or an error occured</para>
        /// </returns>
        User? GetUserByToken(string token);
        /// <summary>
        /// retrieve a user from the database using his userId
        /// </summary>
        /// <param name="userId">the unique id (primary key) of the user</param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that id or an error occured</para>
        /// </returns>
        User? GetUserById(int userId);
        /// <summary>
        /// returns a list of usernames of all users currently in the database
        /// </summary>
        /// <returns>
        /// <para>A list of strings containing the usernames (Primary Key) of each user</para>
        /// </returns>
        List<string> GetListOfUsers();
    }
}