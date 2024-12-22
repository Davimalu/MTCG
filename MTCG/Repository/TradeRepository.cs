using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using MTCG.DAL;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;

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
                DatabaseService.AddParameterWithValue(dbCommand, "@requestedCardType", DbType.String,
                    offer.RequestedMonster ? "Monster" : "Spell");
                DatabaseService.AddParameterWithValue(dbCommand, "@requestedDamage", DbType.Single, offer.RequestedDamage);


                // Execute query and error handling
                int tradeId;

                try
                {
                    tradeId = (int)(dbCommand.ExecuteScalar() ?? 0);
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(
                        $"[ERROR] Trade offer of user {offer.User.Username} couldn't be written to the database!");
                    Console.WriteLine($"[ERROR] {ex.Message}");
                    Console.ResetColor();
                    return -1;
                }

                return tradeId;
            }
        }

        public bool RemoveTradeDeal(TradeOffer offer)
        {
            lock (ThreadSync.DatabaseLock)
            {
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
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[ERROR] Error deleting trade offer of {offer.User.Username} from the database!");
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

        public List<TradeOffer> GetAllTradeDeals()
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT tradeId, userId, cardId, requestedCardType, requestedDamage FROM tradeDeals
                                                                      WHERE active = true
                                                                      """);

                // TODO: Add error handling?
                using IDataReader reader = dbCommand.ExecuteReader();

                List<TradeOffer> deals = new List<TradeOffer>();


                /* Since there is only one database connection that is shared between all threads and queries, two queries cannot run simultaneously
                 * Furthermore, it seems that NPGSQL doesn't support MARS https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/enabling-multiple-active-result-sets
                 * Thus, the functions "GetUserById" and "GetCardById" cannot be called immediately but only after the query to the tradeDeals table is finished
                 */

                // Check if query returned a result
                while (reader.Read())
                {
                    int tradeId = reader.GetInt32(0);
                    int userId = reader.GetInt32(1);
                    string cardId = reader.GetString(2);
                    string requestedCardType = reader.GetString(3);
                    float requestedDamage = reader.GetFloat(4);

                    deals.Add(new TradeOffer()
                        {
                            Id = tradeId,
                            User = new User() {Id = userId},
                            Card = new MonsterCard() {Id = cardId},
                            RequestedMonster = requestedCardType == "Monster" ? true : false,
                            RequestedDamage = requestedDamage
                        });
                }

                return deals;
            }
        }

        public TradeOffer? GetTradeDealById(int tradeId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT tradeId, userId, cardId, requestedCardType, requestedElementType, requestedDamage FROM tradeDeals
                                                                      WHERE active = true AND tradeId = @tradeId
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@tradeId", DbType.Int32, tradeId);

                // TODO: Add error handling?
                using IDataReader reader = dbCommand.ExecuteReader();

                // Check if query returned a result
                if (reader.Read())
                {
                    int userId = reader.GetInt32(1);
                    string cardId = reader.GetString(2);
                    string requestedCardType = reader.GetString(3);
                    ElementType requestedElementType = Enum.Parse<ElementType>(reader.GetString(4));
                    float requestedDamage = reader.GetFloat(5);

                    return (new TradeOffer()
                    {
                        Id = tradeId,
                        User = new User() { Id = userId },
                        Card = new MonsterCard() { Id = cardId },
                        RequestedMonster = requestedCardType == "Monster" ? true : false,
                        RequestedDamage = requestedDamage
                    });
                }

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

                // TODO: Add error handling?
                using IDataReader reader = dbCommand.ExecuteReader();

                // Check if query returned a result
                if (reader.Read())
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

                return null;
            }
        }

        public int SetTradeOfferInactive(int tradeIdToUpdate)
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
                int tradeId;
                try
                {
                    tradeId = (int)(dbCommand.ExecuteScalar() ?? 0);
                }
                catch (Exception ex)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[ERROR] Trade offer with ID {tradeIdToUpdate} couldn't be set to inactive");
                    Console.WriteLine($"[ERROR] {ex.Message}");
                    Console.ResetColor();
                    return -1;
                }

                return tradeId;
            }
        }
    }
}
