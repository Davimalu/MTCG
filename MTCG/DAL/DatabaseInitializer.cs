using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models.Enums;
using System.Data;

namespace MTCG.DAL
{
    public class DatabaseInitializer
    {
        private readonly DatabaseService _databaseService = DatabaseService.Instance;
        private readonly IEventService _eventService = new EventService();

        /// <summary>
        /// creates all PostgresSQL tables the MonsterTradingCardGame needs; drops all existing tables
        /// </summary>
        public void CreateTables()
        {
            DropAllTables();
            CreateUserTable();
            CreateCardsTable();
            CreatePackagesTable();
            CreateUserStacksTable();
            CreateUserDecksTable();
            CreateTradeDealsTable();
            CreateCardsPackagesTable();
        }


        /// <summary>
        /// creates the PostgresSQL table that stores information about all registered users
        /// </summary>
        public void CreateUserTable()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                CREATE TABLE IF NOT EXISTS users (
                    userId SERIAL PRIMARY KEY,
                    username VARCHAR(255) NOT NULL,
                    chosenName VARCHAR (255),
                    biography TEXT,
                    image VARCHAR(255),
                    password VARCHAR(255) NOT NULL,
                    authToken VARCHAR(255),
                    coinCount INT DEFAULT 20,
                    wins INT DEFAULT 0,
                    losses INT DEFAULT 0,
                    ties INT DEFAULT 0,
                    eloPoints INT DEFAULT 100 );
                """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error creating Table `users`", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Info, $"Table `users` created", null);
        }


        /// <summary>
        /// creates the PostgresSQL table that stores all cards that were added to the game
        /// </summary>
        public void CreateCardsTable()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                CREATE TABLE IF NOT EXISTS cards (
                    cardId VARCHAR(255) PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    damage REAL NOT NULL,
                    cardType VARCHAR(255) NOT NULL CHECK (cardType IN ('Monster', 'Spell')),
                    elementType VARCHAR(255) NOT NULL CHECK (elementType IN ('Normal', 'Water', 'Fire'))
                );
                """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error creating Table `cards`", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Info, $"Table `cards` created", null);
        }

        /// <summary>
        /// creates the PostgresSQL table that stores all packages that were added to the game
        /// </summary>
        public void CreatePackagesTable()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS packages (
                                                                     packageId SERIAL PRIMARY KEY
                                                                 );
                                                                 """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error creating Table `packages`", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Info, $"Table `packages` created", null);
        }


        /// <summary>
        /// creates the PostgresSQL table that saves which cards are contained in a certain package
        /// </summary>
        public void CreateCardsPackagesTable()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS cardsPackages (
                                                                     packageId INT NOT NULL,
                                                                     cardId VARCHAR(255) NOT NULL,
                                                                     PRIMARY KEY (cardId, packageId),
                                                                     FOREIGN KEY (packageId) REFERENCES packages(packageId) ON DELETE CASCADE,
                                                                     FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                 );
                                                                 """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error creating Table `cardsPackages`", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Info, $"Table `cardsPackages` created", null);
        }


        /// <summary>
        /// creates the PostgresSQL table that saves which cards a user has in his stack
        /// </summary>
        public void CreateUserStacksTable()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS userStacks (
                                                                     userId INT NOT NULL,
                                                                     cardId VARCHAR(255) NOT NULL,
                                                                     PRIMARY KEY (userId, cardId),
                                                                     FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE,
                                                                     FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                 );
                                                                 """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error creating Table `userStacks`", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Info, $"Table `userStacks` created", null);
        }


        /// <summary>
        /// creates the PostgresSQL table that saves which cards a user has in his deck
        /// </summary>
        public void CreateUserDecksTable()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS userDecks (
                                                                     userId INT NOT NULL,
                                                                     cardId VARCHAR(255) NOT NULL,
                                                                     PRIMARY KEY (userId, cardId),
                                                                     FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE,
                                                                     FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                 );
                                                                 """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error creating Table `userDecks`", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Info, $"Table `userDecks` created", null);
        }


        /// <summary>
        /// creates the PostgresSQL table that stores all open (and closed) trade offers
        /// </summary>
        public void CreateTradeDealsTable()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                  CREATE TABLE IF NOT EXISTS tradeDeals (
                                                                      tradeId SERIAL PRIMARY KEY,
                                                                      userId INT NOT NULL,
                                                                      cardId VARCHAR(255) NOT NULL,
                                                                      requestedCardType VARCHAR(255) NOT NULL,
                                                                      requestedDamage REAL NOT NULL,
                                                                      active BOOLEAN NOT NULL DEFAULT true,
                                                                      FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE,
                                                                      FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                  );
                                                                  """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error creating Table `tradeDeals`", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Info, $"Table `tradeDeals` created", null);
        }

        /// <summary>
        /// drops all PostgresSQL database tables
        /// </summary>
        public void DropAllTables()
        {
            using IDbCommand dbCommand = _databaseService.CreateCommand("""
                DROP TABLE IF EXISTS userStacks;
                DROP TABLE IF EXISTS userDecks;
                DROP TABLE IF EXISTS cardsPackages;
                DROP TABLE IF EXISTS tradeDeals;
                DROP TABLE IF EXISTS users;
                DROP TABLE IF EXISTS cards;
                DROP TABLE IF EXISTS packages;
             """);

            try
            {
                dbCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, "Error dropping tables", ex);
                Environment.Exit(-1);
            }

            _eventService.LogEvent(EventType.Warning, $"All database tables have been dropped", null);
        }
    }
}