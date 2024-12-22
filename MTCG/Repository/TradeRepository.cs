using MTCG.DAL;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Cards;
using MTCG.Models.Enums;
using System.Data;

namespace MTCG.Repository
{
    public class TradeRepository : ITradeRepository
    {
        #region Singleton
        private static TradeRepository? _instance;

        public static TradeRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TradeRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly DatabaseService _databaseService = DatabaseService.Instance;
        private readonly IEventService _eventService = new EventService();


        public int? AddTradeOfferToDatabase(TradeOffer offer)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                        INSERT INTO tradeDeals (userId, cardId, requestedCardType, requestedDamage)
                                                                        VALUES (@userId, @cardId, @requestedCardType, @requestedDamage)
                                                                        RETURNING tradeId;
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, offer.User.Id);
                DatabaseService.AddParameterWithValue(dbCommand, "@cardId", DbType.String, offer.Card.Id);
                DatabaseService.AddParameterWithValue(dbCommand, "@requestedCardType", DbType.String, offer.RequestedMonster ? "Monster" : "Spell");
                DatabaseService.AddParameterWithValue(dbCommand, "@requestedDamage", DbType.Single, offer.RequestedDamage);

                // Execute query and error handling
                int tradeId;

                try
                {
                    tradeId = (int)(dbCommand.ExecuteScalar() ?? 0);
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't write trade offer of user {offer.User.Username} to database", ex);
                    return null;
                }

                return tradeId;
            }
        }


        public bool RemoveTradeOfferFromDatabase(TradeOffer offer)
        {
            lock (ThreadSync.DatabaseLock)
            {
                if (offer.Id == null)
                {
                    return false;
                }

                // Prepare SQL query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      DELETE FROM tradeDeals
                                                                      WHERE tradeId = @tradeId;
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@tradeId", DbType.Int32, offer.Id);

                // Execute query and error handling
                int rowsAffected;

                try
                {
                    rowsAffected = dbCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't remove trade offer of User {offer.User.Username} from the database", ex);
                    return false;
                }

                // Check if at least one row was affected
                if (rowsAffected <= 0)
                {
                    _eventService.LogEvent(EventType.Warning, $"Couldn't remove trade offer of User {offer.User.Username} from the database: Nonexistent Trade Offer", null);
                    return false;
                }

                return true;
            }
        }


        public List<TradeOffer> GetAllTradeOffers()
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT tradeId, userId, cardId, requestedCardType, requestedDamage FROM tradeDeals
                                                                      WHERE active = true
                                                                      """);

                // Execute query and error handling
                IDataReader? reader = null;

                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve list of Trade Offers from database", ex);
                    return new List<TradeOffer>();
                }

                List<TradeOffer> offers = new List<TradeOffer>();

                /* Since there is only one database connection that is shared between all threads and queries, two queries cannot run simultaneously
                 * Furthermore, it seems that NPGSQL doesn't support MARS https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/enabling-multiple-active-result-sets
                 * Thus, the functions "GetUserById" and "GetCardById" cannot be called immediately but only after the query to the tradeDeals table is finished
                 */

                // Check if query returned a result
                while (reader.Read())
                {
                    offers.Add(CreateTradeOfferFromDatabaseEntry(reader));
                }

                reader.Close();
                return offers;
            }
        }


        public TradeOffer? GetTradeOfferById(int tradeId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT tradeId, userId, cardId, requestedCardType, requestedDamage FROM tradeDeals
                                                                      WHERE active = true AND tradeId = @tradeId
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@tradeId", DbType.Int32, tradeId);

                // Execute query and error handling
                IDataReader? reader = null;

                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve trade offer with ID {tradeId} from database", ex);
                    return null;
                }

                // Check if query returned a result
                if (reader.Read())
                {
                    TradeOffer offer = CreateTradeOfferFromDatabaseEntry(reader);
                    reader.Close();
                    return offer;
                }

                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve trade offer with ID {tradeId} from database: Nonexistent trade offer", null);
                reader.Close();
                return null;
            }
        }


        public TradeOffer? GetTradeDealByCardId(string cardIdToLookup)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT tradeId, userId, cardId, requestedCardType, requestedDamage FROM tradeDeals
                                                                      WHERE active = true AND cardId = @cardId
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@cardId", DbType.String, cardIdToLookup);

                // Execute query and error handling
                IDataReader? reader = null;

                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve trade offer associated with Card {cardIdToLookup} from database", ex);
                    return null;
                }

                // Check if query returned a result
                if (reader.Read())
                {
                    TradeOffer offer = CreateTradeOfferFromDatabaseEntry(reader);
                    reader.Close();
                    return offer;
                }

                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve trade offer associated with Card {cardIdToLookup} from database: Nonexistent trade offer", null);
                reader.Close();
                return null;
            }
        }


        public bool SetTradeOfferInactive(int tradeIdToUpdate)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                            UPDATE tradeDeals
                                                                            SET active = false
                                                                            WHERE tradeId = @id
                                                                            """);

                DatabaseService.AddParameterWithValue(dbCommand, "@id", DbType.Int32, tradeIdToUpdate);

                // Execute query and error handling
                try
                {
                    dbCommand.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't set trade offer with ID {tradeIdToUpdate} to inactive", ex);
                    return false;
                }

                return true;
            }
        }


        /// <summary>
        /// creates a TradeOffer from a single database entry in the `tradeDeals` table
        /// </summary>
        /// <param name="reader">IDataReader of a query which is currently reading an entry from the `tradeDeals` table</param>
        /// <returns>TradeOffer object containing the information stored in the database</returns>
        private TradeOffer CreateTradeOfferFromDatabaseEntry(IDataReader reader)
        {
            int tradeId = reader.GetInt32(0);
            int userId = reader.GetInt32(1);
            string cardId = reader.GetString(2);
            string requestedCardType = reader.GetString(3);
            float requestedDamage = reader.GetFloat(4);

            return (new TradeOffer()
            {
                Id = tradeId,
                User = new User() { Id = userId },
                Card = new MonsterCard() { Id = cardId },
                RequestedMonster = requestedCardType == "Monster" ? true : false,
                RequestedDamage = requestedDamage
            });
        }
    }
}
