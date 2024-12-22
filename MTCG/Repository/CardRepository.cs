using MTCG.DAL;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using Npgsql;
using System.Data;

namespace MTCG.Repository
{
    public class CardRepository : ICardRepository
    {
        #region Singleton
        private static CardRepository? _instance;

        public static CardRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CardRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly DatabaseService _databaseService = DatabaseService.Instance;
        private readonly IEventService _eventService = new EventService();
        private readonly RepositoryHelper _repositoryHelper = new RepositoryHelper();


        public bool AddCardToDatabase(Card card)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                            INSERT INTO cards (cardId, name, damage, cardType, elementType)
                                                                            VALUES (@id, @name, @damage, @cardType, @elementType);
                                                                            """);

                DatabaseService.AddParameterWithValue(dbCommand, "@id", DbType.String, card.Id);
                DatabaseService.AddParameterWithValue(dbCommand, "@name", DbType.String, card.Name);
                DatabaseService.AddParameterWithValue(dbCommand, "@damage", DbType.Int32, card.Damage);
                DatabaseService.AddParameterWithValue(dbCommand, "@cardType", DbType.String, card is MonsterCard ? "Monster" : "Spell");
                DatabaseService.AddParameterWithValue(dbCommand, "@elementType", DbType.String, card.ElementType.ToString());

                // Execute query and catch errors
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch (PostgresException ex) when (ex.SqlState == "23505")
                {
                    // Handle duplicate key error | For a duplicate key error, the SQL state is 23505
                    _eventService.LogEvent(EventType.Warning, $"Card {card.Name} already exists in the database", ex);
                    return true;
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Card {card.Name} couldn't be added to database", ex);
                    return false;
                }

                return true;
            }
        }


        public Card? GetCardById(string cardId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                            SELECT cardId, name, damage, cardType, elementType
                                                                            FROM cards
                                                                            WHERE cardId = @cardId
                                                                            """);
                DatabaseService.AddParameterWithValue(dbCommand, "@cardId", DbType.String, cardId);

                // Execute query and error handling
                IDataReader? reader = null;
                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve card with ID {cardId} from Database", ex);
                    return null;
                }

                // Check if query returned a result
                if (reader.Read())
                {
                    Card cardToReturn = _repositoryHelper.CreateCardFromDatabaseEntry(reader);
                    reader.Close();
                    return cardToReturn;
                }

                // Return null if no results
                reader.Close();
                return null;
            }
        }
    }
}
