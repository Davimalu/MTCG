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
        }

        public void CreateUserTable()
        {
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(255) NOT NULL,
                    password VARCHAR(255) NOT NULL,
                    authToken VARCHAR(255),
                    coinCount INT DEFAULT 20,
                    eloPoints INT DEFAULT 100 );
                """);

            dbCommand.ExecuteNonQuery();

            Console.WriteLine("[INFO] Users Table created!");
        }

        public void CreateCardsTable()
        {
            // TODO: Replace type with enum
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
                CREATE TABLE IF NOT EXISTS cards (
                    id VARCHAR(255) PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    damage INT NOT NULL,
                    type VARCHAR(255) NOT NULL
                );
                """);

            dbCommand.ExecuteNonQuery();
            Console.WriteLine("[INFO] Cards Table created!");
        }

        public void DropAllTables()
        {
            using IDbCommand dbCommand = dataLayer.CreateCommand("""
                DROP TABLE IF EXISTS users;
                DROP TABLE IF EXISTS cards;
             """);

            dbCommand.ExecuteNonQuery();

            Console.WriteLine("[INFO] All tables dropped!");
        }
    }
}