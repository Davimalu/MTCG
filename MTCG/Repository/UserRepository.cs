using MTCG.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MTCG.Models;
using System.Data;
using MTCG.Interfaces;

namespace MTCG.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly DataLayer _dataLayer = DataLayer.Instance;

        public void AddUser(User user)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                INSERT INTO users (username, password, authToken, coinCount)
                VALUES (@username, @password, @authToken, @coinCount);
                """);

            DataLayer.AddParameterWithValue(dbCommand, "@username", DbType.String, user.Username);
            DataLayer.AddParameterWithValue(dbCommand, "@password", DbType.String, user.Password);
            DataLayer.AddParameterWithValue(dbCommand, "@authToken", DbType.String, user.AuthToken);
            DataLayer.AddParameterWithValue(dbCommand, "@coinCount", DbType.Int32, user.CoinCount);
            DataLayer.AddParameterWithValue(dbCommand, "@eloPoints", DbType.Int32, user.EloPoints);

            user.Id = (int)(dbCommand.ExecuteScalar() ?? 0);

            Console.WriteLine($"[INFO] User {user.Username} added to database!");
        }

        public User? GetUserById(int id)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                SELECT id, username, password, authToken, coinCount, elopoints
                FROM users
                WHERE id = @id
                """);
            DataLayer.AddParameterWithValue(dbCommand, "@id", DbType.Int32, id);

            using IDataReader reader = dbCommand.ExecuteReader();
            if (reader.Read())
            {
                Console.WriteLine($"[INFO] User {reader.GetString(1)} retrieved from database!");

                return new User()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    AuthToken = reader.GetString(3),
                    CoinCount = reader.GetInt32(4),
                    EloPoints = reader.GetInt32(5)
                };
            }

            return null;
        }

        public User? GetUserByName(string username)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                SELECT id, username, password, authToken, coinCount, elopoints
                FROM users
                WHERE username = @username
                """);
            DataLayer.AddParameterWithValue(dbCommand, "@username", DbType.String, username);

            using IDataReader reader = dbCommand.ExecuteReader();
            if (reader.Read())
            {
                Console.WriteLine($"[INFO] User {reader.GetString(1)} retrieved from database!");

                return new User()
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    Password = reader.GetString(2),
                    AuthToken = reader.GetString(3),
                    CoinCount = reader.GetInt32(4),
                    EloPoints = reader.GetInt32(5)
                };
            }

            return null;
        }

        public void UpdateUser(User user)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                UPDATE users
                SET username = @username, password = @password, authToken = @authToken, coinCount = @coinCount, eloPoints = @eloPoints
                WHERE id = @id
                """);

            DataLayer.AddParameterWithValue(dbCommand, "@username", DbType.String, user.Username);
            DataLayer.AddParameterWithValue(dbCommand, "@password", DbType.String, user.Password);
            DataLayer.AddParameterWithValue(dbCommand, "@authToken", DbType.String, user.AuthToken);
            DataLayer.AddParameterWithValue(dbCommand, "@cointCount", DbType.Int32, user.CoinCount);
            DataLayer.AddParameterWithValue(dbCommand, "@eloPoints", DbType.Int32, user.EloPoints);
            DataLayer.AddParameterWithValue(dbCommand, "@id", DbType.Int32, user.Id);

            dbCommand.ExecuteNonQuery();
        }
    }
}
