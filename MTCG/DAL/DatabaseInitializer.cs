using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.DAL
{
    public class DatabaseInitializer
    {
        private readonly DataLayer _dataLayer = DataLayer.Instance;

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

        // Saves all users that registered
        public void CreateUserTable()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
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

            dbCommand.ExecuteNonQuery();

            Console.WriteLine("[INFO] Users Table created!");
        }

        // Saves all cards that are available
        public void CreateCardsTable()
        {
            // TODO: Replace type with enum
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                CREATE TABLE IF NOT EXISTS cards (
                    cardId VARCHAR(255) PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    damage REAL NOT NULL,
                    cardType VARCHAR(255) NOT NULL,
                    elementType VARCHAR(255) NOT NULL
                );
                """);

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] Cards Table created!");
        }

        // Saves all packages that were created
        public void CreatePackagesTable()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS packages (
                                                                     packageId SERIAL PRIMARY KEY
                                                                 );
                                                                 """);

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] Packages Table created!");
        }

        // Saves which cards are contained by a package
        public void CreateCardsPackagesTable()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS cardsPackages (
                                                                     packageId INT NOT NULL,
                                                                     cardId VARCHAR(255) NOT NULL,
                                                                     PRIMARY KEY (cardId, packageId),
                                                                     FOREIGN KEY (packageId) REFERENCES packages(packageId) ON DELETE CASCADE,
                                                                     FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                 );
                                                                 """);

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] CardsPackages Table created!");
        }

        // Saves which cards a user has in his stack
        public void CreateUserStacksTable()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS userStacks (
                                                                     userId INT NOT NULL,
                                                                     cardId VARCHAR(255) NOT NULL,
                                                                     PRIMARY KEY (userId, cardId),
                                                                     FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE,
                                                                     FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                 );
                                                                 """);

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] UserStacks Table created!");
        }

        // Saves which cards a user has in his deck
        public void CreateUserDecksTable()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS userDecks (
                                                                     userId INT NOT NULL,
                                                                     cardId VARCHAR(255) NOT NULL,
                                                                     PRIMARY KEY (userId, cardId),
                                                                     FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE,
                                                                     FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                 );
                                                                 """);

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] UserDecks Table created!");
        }

        // Saves all open trade deals
        public void CreateTradeDealsTable()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
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

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] TradeDeals Table created!");
        }

        public void DropAllTables()
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                DROP TABLE IF EXISTS userStacks;
                DROP TABLE IF EXISTS userDecks;
                DROP TABLE IF EXISTS cardsPackages;
                DROP TABLE IF EXISTS tradeDeals;
                DROP TABLE IF EXISTS users;
                DROP TABLE IF EXISTS cards;
                DROP TABLE IF EXISTS packages;
             """);

            dbCommand.ExecuteNonQuery();

            Console.WriteLine("[INFO] All tables dropped!");
        }
    }
}