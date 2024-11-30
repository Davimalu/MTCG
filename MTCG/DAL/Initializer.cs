using MTCG.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.DAL
{
    public class Initializer
    {
        private readonly DataLayer dataLayer;

        public Initializer()
        {
            dataLayer = DataLayer.Instance;
        }

        public void CreateTables()
        {
            DropAllTables();
            CreateUserTable();
            CreateCardsTable();
            CreatePackagesTable();
            CreateUserCardsTable();
            CreateCardsPackagesTable();
        }

        // Saves all users that registered
        public void CreateUserTable()
        {
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
                CREATE TABLE IF NOT EXISTS users (
                    userId SERIAL PRIMARY KEY,
                    username VARCHAR(255) NOT NULL,
                    password VARCHAR(255) NOT NULL,
                    authToken VARCHAR(255),
                    coinCount INT DEFAULT 20,
                    eloPoints INT DEFAULT 100 );
                """);

            dbCommand.ExecuteNonQuery();

            Console.WriteLine("[INFO] Users Table created!");
        }

        // Saves all cards that are available
        public void CreateCardsTable()
        {
            // TODO: Replace type with enum
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
                CREATE TABLE IF NOT EXISTS cards (
                    cardId VARCHAR(255) PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    damage INT NOT NULL,
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
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
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
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
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

        // Saves which cards a user owns
        public void CreateUserCardsTable()
        {
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
                                                                 CREATE TABLE IF NOT EXISTS userCards (
                                                                     userId INT NOT NULL,
                                                                     cardId VARCHAR(255) NOT NULL,
                                                                     PRIMARY KEY (userId, cardId),
                                                                     FOREIGN KEY (userId) REFERENCES users(userId) ON DELETE CASCADE,
                                                                     FOREIGN KEY (cardId) REFERENCES cards(cardId) ON DELETE CASCADE
                                                                 );
                                                                 """);

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] UserCards Table created!");
        }

        public void DropAllTables()
        {
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
                DROP TABLE IF EXISTS userCards;
                DROP TABLE IF EXISTS cardsPackages;
                DROP TABLE IF EXISTS users;
                DROP TABLE IF EXISTS cards;
                DROP TABLE IF EXISTS packages;
             """);

            dbCommand.ExecuteNonQuery();

            Console.WriteLine("[INFO] All tables dropped!");
        }
    }
}