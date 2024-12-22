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
        private readonly RepositoryHelper _repositoryHelper = new RepositoryHelper();
        private readonly IEventService _eventService = new EventService();


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
                    _eventService.LogEvent(EventType.Error, $"Couldn't add package to database", ex);
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


        public bool AddCardsToPackage(Package package)
        {
            lock (ThreadSync.DatabaseLock)
            {
                if (package.Id == null)
                {
                    _eventService.LogEvent(EventType.Warning, $"`AddCardsToPackage` must not be called on it's own! The function must be executed as part of `AddPackageToDatabase`", null);
                    return false;
                }

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
                        DeleteAllPackageAssociations((int)package.Id); // Also delete all other associations that may have been saved before this failed transaction
                        return false;
                    }

                    // Check if query executed successfully
                    if (rowsAffected <= 0)
                    {
                        _eventService.LogEvent(EventType.Error, $"Card {card.Name} couldn't be associated with package", null);
                        DeleteAllPackageAssociations((int)package.Id); // Also delete all other associations that may have been saved before this failed transaction
                        return false;
                    }
                }

                return true;
            }
        }


        /// <summary>
        /// deletes all associations of a package identified by its packageId with its cards
        /// </summary>
        /// <param name="packageId">the packageId of the package</param>
        /// <returns>
        /// <para>true on success</para>
        /// <para>false on error</para>
        /// </returns>
        private bool DeleteAllPackageAssociations(int packageId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                            DELETE FROM cardsPackages
                                                                            WHERE packageId = @packageId;
                                                                            """);

                DatabaseService.AddParameterWithValue(dbCommand, "@packageId", DbType.Int32, packageId);

                // Execute query and error handling
                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't delete associations of package {packageId} from the database", ex);
                    return false;
                }

                return true;
            }
        }


        public int? GetIdOfRandomPackage()
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT packageId FROM packages
                                                                      ORDER BY RANDOM()
                                                                      LIMIT 1
                                                                      """);

                // Execute query and error handling
                IDataReader? reader = null;
                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve random package from Database", ex);
                    return null;
                }

                // See if there any results
                if (reader.Read())
                {
                    int randomPackageId = reader.GetInt32(0);
                    reader.Close();
                    return randomPackageId;
                }

                _eventService.LogEvent(EventType.Warning, $"Couldn't retrieve random package from Database: No packages available", null);

                reader.Close();
                return null;
            }
        }


        public Package? GetPackageById(int packageId)
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


                // Execute query and error handling
                IDataReader? reader = null;

                try
                {
                    reader = dbCommand.ExecuteReader();
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Couldn't retrieve package with ID {packageId} from Database", ex);
                    return null;
                }

                List<Card> cards = new List<Card>(5);

                // Create a card object out of each entry that was returned
                while (reader.Read())
                {
                    cards.Add(_repositoryHelper.CreateCardFromDatabaseEntry(reader));
                }

                // If the package doesn't exist or has no cards associated with it...
                if (cards.Count == 0)
                {
                    reader.Close();
                    return null;
                }

                reader.Close();
                return new Package()
                {
                    Id = packageId,
                    Cards = cards
                };
            }
        }


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
                    _eventService.LogEvent(EventType.Error, $"Couldn't remove Package with ID {packageId} from the database", ex);
                    return false;
                }

                // Check if at least one row was affected
                if (rowsAffected <= 0)
                {
                    _eventService.LogEvent(EventType.Warning, $"Couldn't remove Package with ID {packageId} from the database", null);
                    return false;
                }

                return true;
            }
        }
    }
}
