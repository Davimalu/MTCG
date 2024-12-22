using MTCG.DAL;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using System.Data;
using MTCG.Models.Enums;

namespace MTCG.Repository
{
    public class DeckRepository : IDeckRepository
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
        private readonly IEventService _eventService = new EventService();


        public bool SaveDeckOfUserToDatabase(User user)
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
                        _eventService.LogEvent(EventType.Error, $"Couldn't write Deck of user {user.Username} to the database", ex);
                        return false;
                    }

                    // Deck empty or user didn't exist
                    if (rowsAffected <= 0)
                    {
                        _eventService.LogEvent(EventType.Warning, $"Couldn't write Deck of user {user.Username} to the database", null);
                        return false;
                    }
                }

                return true;
            }
        }


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

                // Execute query and error handling
                IDataReader? reader = null;
                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve cards of User {user.Username}'s Deck", ex);
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
                    _eventService.LogEvent(EventType.Error, $"Couldn't delete deck of User {user.Username} from the database", ex);
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
