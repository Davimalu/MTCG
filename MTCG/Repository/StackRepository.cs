using MTCG.DAL;
using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Repository
{
    public class StackRepository
    {
        #region Singleton
        private static StackRepository? _instance;

        public static StackRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StackRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly DataLayer _dataLayer = DataLayer.Instance;

        /// <summary>
        /// delete the stack of a user form the database
        /// </summary>
        /// <param name="user">user object, must contain at least the userId</param>
        /// <returns>
        /// <para>true on success</para>
        /// <para>false if user or his stack were not yet added to database or on error</para>
        /// </returns>
        public bool ClearUserStack(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      DELETE FROM userStacks
                                                                      WHERE userid = @userId;
                                                                      """);

                DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

                // Execute query and error handling
                int rowsAffected;

                try
                {
                    rowsAffected = dbCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[ERROR] Error deleting stack of {user.Username} from the database!");
                    Console.WriteLine($"[ERROR] {ex.Message}");
                    Console.ResetColor();
                    return false;
                }

                // Check if at least one row was affected
                if (rowsAffected <= 0)
                {
                    return false;
                }

                return true;
            }
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
            lock (ThreadSync.DatabaseLock)
            {
                // For each card in the users stack...
                foreach (var card in user.Stack.Cards)
                {
                    // Prepare SQL query
                    using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                          INSERT INTO userStacks (userId, cardId)
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
        }

        /// <summary>
        /// retrieves all cards of a user's stack
        /// </summary>
        /// <param name="user"></param>
        /// <returns>a list of all the cardIds of the cards from the user's stack</returns>
        public List<string>? GetCardIdsOfUserStack(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      SELECT cardId FROM userStacks
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
}
