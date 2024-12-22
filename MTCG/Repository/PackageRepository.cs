using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.DAL;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;

namespace MTCG.Repository
{
    public class PackageRepository : IPackageRepository
    {
        #region Singleton
        private static PackageRepository? _instance;

        public static PackageRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PackageRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly DatabaseService _databaseService = DatabaseService.Instance;
        private readonly IEventService _eventService = new EventService();

        /// <summary>
        /// saves a new package to the database
        /// </summary>
        /// <param name="package">package object containing all cards that belong to this package</param>
        /// <returns>
        ///<para>true if package was added successfully</para>
        ///<para>false if package couldn't be added to database</para>
        /// </returns>
        public bool AddPackageToDatabase(Package package)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("INSERT INTO packages DEFAULT VALUES RETURNING packageId;");

                // Execute query and save packageId of the newly created entry
                try
                {
                    package.Id = (int)(dbCommand.ExecuteScalar() ?? 0);
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Error adding package to database", ex);
                    return false;
                }

                // Check if packageId was returned
                if (package.Id == 0)
                {
                    _eventService.LogEvent(EventType.Warning, $"Unknown error while adding package to database", null);
                    return false;
                }

                _eventService.LogEvent(EventType.Highlight, $"New package with ID {package.Id} added to database", null);
                return AddCardsToPackage(package);
            }
        }

        /// <summary>
        /// saves the information on which cards belong to this package in the database
        /// </summary>
        /// <param name="package">package object containing the packageId of the database entry in table 'packages' and all cards that belong to this package</param>
        /// <returns>
        ///<para>true if all associations were successfully saved to the database</para>
        /// <para>false if at least one association couldn't be saved in the database</para>
        /// </returns>
        public bool AddCardsToPackage(Package package)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // for all cards in the package...
                foreach (var card in package.Cards)
                {
                    // Prepare SQL query
                    using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                          INSERT INTO cardsPackages (packageId, cardId)
                                                                          VALUES (@packageId, @cardId);
                                                                          """);

                    DatabaseService.AddParameterWithValue(dbCommand, "@packageId", DbType.Int32, package.Id);
                    DatabaseService.AddParameterWithValue(dbCommand, "@cardId", DbType.String, card.Id);

                    // Execute query and error handling
                    int rowsAffected;

                    try
                    {
                        rowsAffected = dbCommand.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _eventService.LogEvent(EventType.Error, $"Card {card.Name} couldn't be associated with package", ex);
                        // TODO: Delete package
                        return false;
                    }

                    // Check if query executed successfully
                    if (rowsAffected <= 0)
                    {
                        _eventService.LogEvent(EventType.Error, $"Card {card.Name} couldn't be associated with package", null);
                        // TODO: Delete package | DELETE WHERE Id = packageId
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// gets the packageId of a random package from the packages table
        /// </summary>
        /// <returns>
        /// <para>packageId of a random package on success</para>
        /// <para>0 if no packages available</para>
        /// </returns>
        public int GetRandomPackageId()
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT packageId FROM packages
                                                                      ORDER BY RANDOM()
                                                                      LIMIT 1
                                                                      """);

                // TODO: Add error handling?
                using IDataReader reader = dbCommand.ExecuteReader();

                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }

                return 0;
            }
        }

        /// <summary>
        /// retrieves a package from the database using its packageId
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns>
        /// <para>package containing all metadata and associated cards on success</para>
        /// <para>null if the package doesn't exist or has no cards associated with it</para>
        /// </returns>
        public Package? GetPackageFromId(int packageId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT cards.cardId, name, damage, cardType, elementType FROM cards
                                                                      JOIN cardsPackages USING (cardId)
                                                                      WHERE cardsPackages.packageId = @packageId
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@packageId", DbType.Int32, packageId);

                // TODO: Add error handling?
                using IDataReader reader = dbCommand.ExecuteReader();

                List<Card> cards = new List<Card>(5);

                // Create a card object out of each entry that was returned
                while (reader.Read())
                {
                    string id = reader.GetString(0);
                    string name = reader.GetString(1);
                    float damage = reader.GetFloat(2);
                    ElementType elementType = Enum.Parse<ElementType>(reader.GetString(4));

                    // Generate appropriate card type
                    if (reader.GetString(3) == "Spell") // Spell Card
                    {
                        cards.Add(new SpellCard(id, name, damage, elementType));
                    }
                    else // Monster Card
                    {
                        cards.Add(new MonsterCard(id, name, damage, elementType));
                    }
                }

                // If the package doesn't exist or has no cards associated with it...
                if (cards.Count == 0)
                {
                    return null;
                }

                return new Package()
                {
                    Id = packageId,
                    Cards = cards
                };
            }
        }

        /// <summary>
        /// delete a package from the database using its packageId; does NOT delete the cards that were added with that package
        /// </summary>
        /// <param name="packageId"></param>
        /// <returns>
        /// <para>true if package was deleted successfully</para>
        /// <para>false if there was no package with that packageId or if SQL error occured</para>
        /// </returns>
        public bool DeletePackageById(int packageId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      DELETE FROM packages
                                                                      WHERE packageId = @packageId;
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@packageId", DbType.Int32, packageId);

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
                    Console.WriteLine($"[ERROR] Package with ID {packageId} couldn't be removed from database!");
                    Console.WriteLine($"[ERROR] {ex.Message}");
                    Console.ResetColor();
                    return false;
                }

                // Check if at least one row was affected
                if (rowsAffected <= 0)
                {
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[ERROR] Package with ID {packageId} couldn't be removed from database!");
                    Console.ResetColor();
                    return false;
                }

                return true;
            }
        }
    }
}
