using MTCG.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;
using System.Data;
using MTCG.Interfaces;

namespace MTCG.Repository
{
    public class UserRepository : IUserRepository
    {
        #region Singleton
        private static UserRepository? _instance;

        public static UserRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly CardRepository _cardRepository = CardRepository.Instance;
        private readonly DataLayer _dataLayer = DataLayer.Instance;

        /// <summary>
        /// saves a new user to the database
        /// </summary>
        /// <param name="user">user object containing all user data, Stack and Deck are optional/ignored</param>
        /// <returns>
        /// <para>ID of the newly created database entry on success</para>
        /// <para>-1 on error</para>
        /// </returns>
        public int SaveUserToDatabase(User user)
        {
            // Prepare SQL Query
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                INSERT INTO users (username, password, authToken, coinCount)
                VALUES (@username, @password, @authToken, @coinCount)
                RETURNING userId;
                """);

            DataLayer.AddParameterWithValue(dbCommand, "@username", DbType.String, user.Username);
            DataLayer.AddParameterWithValue(dbCommand, "@password", DbType.String, user.Password);
            DataLayer.AddParameterWithValue(dbCommand, "@authToken", DbType.String, user.AuthToken);
            DataLayer.AddParameterWithValue(dbCommand, "@coinCount", DbType.Int32, user.CoinCount);
            DataLayer.AddParameterWithValue(dbCommand, "@eloPoints", DbType.Int32, user.EloPoints);

            // Execute query and error handling
            try
            {
                user.Id = (int)(dbCommand.ExecuteScalar() ?? 0);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[ERROR] User {user.Username} couldn't be added to the database!");
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();
                return -1;
            }

            Console.WriteLine($"[INFO] User {user.Username} added to database!");
            return user.Id;
        }

        /// <summary>
        /// retrieve a user from the database using his name
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that name or an error occured</para>
        /// </returns>
        public User? GetUserByName(string username)
        {
            // Prepare SQL statement
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                SELECT userId, username, password, authToken, coinCount, eloPoints
                FROM users
                WHERE username = @username
                """);
            DataLayer.AddParameterWithValue(dbCommand, "@username", DbType.String, username);

            // Execute query
            // TODO: Add error handling?
            using IDataReader reader = dbCommand.ExecuteReader();

            if (reader.Read())
            {
                return new User()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    AuthToken = reader.GetString(3),
                    CoinCount = reader.GetInt32(4),
                    EloPoints = reader.GetInt32(5)
                };
            }

            // If no entries were returned...
            return null;
        }

        /// <summary>
        /// update the information of an existing user in the users table; does NOT update the users stack or deck
        /// </summary>
        /// <param name="user">user object containing the updated user data, Stack and Deck are optional/ignored</param>
        /// <returns>
        /// <para>ID of the updated database entry on success</para>
        /// <para>-1 if the user has not yet been added to the database or on error</para>
        /// </returns>
        public int UpdateUser(User user)
        {
            // Prepare SQL Query
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                UPDATE users
                SET username = @username, password = @password, authToken = @authToken, coinCount = @coinCount, eloPoints = @eloPoints
                WHERE userId = @id
                RETURNING userId
                """);

            DataLayer.AddParameterWithValue(dbCommand, "@username", DbType.String, user.Username);
            DataLayer.AddParameterWithValue(dbCommand, "@password", DbType.String, user.Password);
            DataLayer.AddParameterWithValue(dbCommand, "@authToken", DbType.String, user.AuthToken);
            DataLayer.AddParameterWithValue(dbCommand, "@coinCount", DbType.Int32, user.CoinCount);
            DataLayer.AddParameterWithValue(dbCommand, "@eloPoints", DbType.Int32, user.EloPoints);
            DataLayer.AddParameterWithValue(dbCommand, "@id", DbType.Int32, user.Id);

            // Execute query and error handling
            int userId;
            try
            {
                userId = dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[ERROR] Changes for user {user.Username} couldn't be written to the database!");
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.ResetColor();
                return -1;
            }

            return userId;
        }

        /// <summary>
        /// retrieve a user from the database using his token
        /// </summary>
        /// <param name="token">the authentication token in format "xxx-mtcgToken"</param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that token or an error occured</para>
        /// </returns>
        public User? GetUserByToken(string token)
        {
            // Prepare SQL query
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT userId, username, password, authToken, coinCount, eloPoints
                                                                  FROM users
                                                                  WHERE authToken = @token
                                                                  """);
            DataLayer.AddParameterWithValue(dbCommand, "@token", DbType.String, token);

            // Execute query
            // TODO: Add error handling?
            using IDataReader reader = dbCommand.ExecuteReader();

            if (reader.Read())
            {
                return new User()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    AuthToken = reader.GetString(3),
                    CoinCount = reader.GetInt32(4),
                    EloPoints = reader.GetInt32(5)
                };
            }

            // If no entries were returned...
            return null;
        }

        public bool ClearUserStack(User user)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  DELETE FROM userCards
                                                                  WHERE userid = @userId;
                                                                  """);

            DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

            int rowsAffected = dbCommand.ExecuteNonQuery();

            if (rowsAffected <= 0)
            {
                Console.WriteLine($"[Error] Cards of User {user.Username} couldn't be removed from database!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// saves the stack of a user to the database
        /// </summary>
        /// <param name="user">user object containing his stack object</param>
        /// <returns>
        /// <para>true if the users stack was successfully stored in the database</para>
        /// <para>false if the user was not yet added to the database, the stack was empty or an error occured</para>
        /// </returns>
        public bool SaveStackOfUser(User user)
        {
            // For each card in the users stack...
            foreach (var card in user.Stack.Cards)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      INSERT INTO userCards (userId, cardId)
                                                                      VALUES (@userId, @cardId);
                                                                      """);

                DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);
                DataLayer.AddParameterWithValue(dbCommand, "@cardId", DbType.String, card.Id);

                // Execute query and handle errors
                int rowsAffected;

                try
                {
                    rowsAffected = dbCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[ERROR] Stack of user {user.Username} couldn't be written to the database!");
                    Console.WriteLine($"[ERROR] {ex.Message}");
                    Console.ResetColor();
                    return false;
                }

                // Stack empty or user didn't exist
                if (rowsAffected <= 0)
                {
                    Console.WriteLine($"[WARNING] Stack of user {user.Username} couldn't be written to the database!");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// retrieves all cards of a user's stack
        /// </summary>
        /// <param name="user"></param>
        /// <returns>a list of all the cardIds of the cards from the user's stack</returns>
        public List<string>? GetCardIdsOfUserStack(User user)
        {
            // Prepare SQL query
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT cardId FROM userCards
                                                                  WHERE userId = @userId
                                                                  """);

            DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

            // Execute query
            // TODO: Add error handling?
            using IDataReader reader = dbCommand.ExecuteReader();

            // Add each entry to the list of cardIds
            List<string> cardsOfUser = new List<string>();

            while (reader.Read())
            {
                cardsOfUser.Add(reader.GetString(0));
            }

            return cardsOfUser;
        }
    }
}
