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
    public class DeckRepository
    {
        #region Singleton
        private static DeckRepository? _instance;

        public static DeckRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DeckRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly DatabaseService _databaseService = DatabaseService.Instance;

        // TODO: The Deck functions are almost the same as all the stack functions -> Can this be written shorter somehow?
        // TODO: Refactor Deck and Stack functions into Stack- and Deck-Repository?

        /// <summary>
        /// saves the deck of a user to the database
        /// </summary>
        /// <param name="user">user object containing his deck object</param>
        /// <returns>
        /// <para>true if the users deck was successfully stored in the database</para>
        /// <para>false if the user was not yet added to the database, the deck was empty or an error occured</para>
        /// </returns>
        public bool SaveDeckOfUser(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // For each card in the users deck...
                foreach (var card in user.Deck.Cards)
                {
                    // Prepare SQL query
                    using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                          INSERT INTO userDecks (userId, cardId)
                                                                          VALUES (@userId, @cardId);
                                                                          """);

                    DatabaseService.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);
                    DatabaseService.AddParameterWithValue(dbCommand, "@cardId", DbType.String, card.Id);

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
                        Console.WriteLine($"[ERROR] Deck of user {user.Username} couldn't be written to the database!");
                        Console.WriteLine($"[ERROR] {ex.Message}");
                        Console.ResetColor();
                        return false;
                    }

                    // Stack empty or user didn't exist
                    if (rowsAffected <= 0)
                    {
                        Console.WriteLine($"[WARNING] Deck of user {user.Username} couldn't be written to the database!");
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// retrieves all cards of a user's deck
        /// </summary>
        /// <param name="user"></param>
        /// <returns>a list of all the cardIds of the cards from the user's deck</returns>
        public List<string>? GetCardIdsOfUserDeck(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT cardId FROM userDecks
                                                                      WHERE userId = @userId
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

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

        /// <summary>
        /// delete the deck of a user form the database
        /// </summary>
        /// <param name="user">user object, must contain at least the userId</param>
        /// <returns>
        /// <para>true on success</para>
        /// <para>false if user or his deck were not yet added to database or on error</para>
        /// </returns>
        public bool ClearUserDeck(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      DELETE FROM userDecks
                                                                      WHERE userid = @userId;
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

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
                    Console.WriteLine($"[ERROR] Error deleting deck of {user.Username} from the database!");
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
    }
}
