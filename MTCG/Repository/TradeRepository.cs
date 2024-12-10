using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using MTCG.DAL;
using MTCG.Logic;
using MTCG.Models;

namespace MTCG.Repository
{
    public class TradeRepository
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

        private readonly DataLayer _dataLayer = DataLayer.Instance;

        public int? AddTradeDeal(TradeDeal deal)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Query
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                        INSERT INTO tradeDeals (userId, cardId, requestedCardType, requestedDamage)
                                                                        VALUES (@userId, @cardId, @requestedCardType, @requestedDamage)
                                                                        RETURNING tradeId;
                                                                      """);

                DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, deal.User.Id);
                DataLayer.AddParameterWithValue(dbCommand, "@cardId", DbType.String, deal.Card.Id);
                DataLayer.AddParameterWithValue(dbCommand, "@requestedCardType", DbType.String,
                    deal.RequestedMonster ? "Monster" : "Spell");
                DataLayer.AddParameterWithValue(dbCommand, "@requestedDamage", DbType.Single, deal.RequestedDamage);


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
                        $"[ERROR] Trade deal of user {deal.User.Username} couldn't be written to the database!");
                    Console.WriteLine($"[ERROR] {ex.Message}");
                    Console.ResetColor();
                    return -1;
                }

                return tradeId;
            }
        }

        public bool RemoveTradeDeal(TradeDeal deal)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      DELETE FROM tradeDeals
                                                                      WHERE tradeId = @tradeId;
                                                                      """);

                DataLayer.AddParameterWithValue(dbCommand, "@tradeId", DbType.Int32, deal.Id);

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
                    Console.WriteLine($"[ERROR] Error deleting trade deal of {deal.User.Username} from the database!");
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

        public List<TradeDeal> GetAllTradeDeals()
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      SELECT tradeId, userId, cardId, requestedCardType, requestedDamage FROM tradeDeals
                                                                      WHERE active = true
                                                                      """);

                // TODO: Add error handling?
                using IDataReader reader = dbCommand.ExecuteReader();

                List<TradeDeal> deals = new List<TradeDeal>();


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

                    deals.Add(new TradeDeal()
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

        public TradeDeal? GetTradeDealById(int tradeId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      SELECT tradeId, userId, cardId, requestedCardType, requestedElementType, requestedDamage FROM tradeDeals
                                                                      WHERE active = true AND tradeId = @tradeId
                                                                      """);

                DataLayer.AddParameterWithValue(dbCommand, "@tradeId", DbType.Int32, tradeId);

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

                    return (new TradeDeal()
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
    }
}
