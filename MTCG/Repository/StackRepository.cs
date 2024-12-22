using MTCG.DAL;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using System.Data;

namespace MTCG.Repository
{
    public class StackRepository : IStackRepository
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

        private readonly DatabaseService _databaseService = DatabaseService.Instance;
        private readonly IEventService _eventService = new EventService();


        public bool SaveStackOfUser(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // For each card in the users stack...
                foreach (var card in user.Stack.Cards)
                {
                    // Prepare SQL query
                    using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                          INSERT INTO userStacks (userId, cardId)
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
                        _eventService.LogEvent(EventType.Error, $"Couldn't write Stack of user {user.Username} to the database", ex);
                        return false;
                    }

                    // Stack empty or user didn't exist
                    if (rowsAffected <= 0)
                    {
                        _eventService.LogEvent(EventType.Warning, $"Couldn't write Stack of user {user.Username} to the database", null);
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
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT cardId FROM userStacks
                                                                      WHERE userId = @userId
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

                // Execute query and error handling
                IDataReader? reader = null;
                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve cards of User {user.Username}'s Stack", ex);
                    return null;
                }

                // Add each entry to the list of cardIds
                List<string> cardsOfUser = [];

                while (reader.Read())
                {
                    cardsOfUser.Add(reader.GetString(0));
                }

                reader.Close();
                return cardsOfUser;
            }
        }


        public bool ClearUserStack(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                            DELETE FROM userStacks
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
                    _eventService.LogEvent(EventType.Error, $"Couldn't delete Stack of User {user.Username} from the database", ex);
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
