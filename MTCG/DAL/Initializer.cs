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
            CreateUserTable();
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
    }
}