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
        #region Singleton
        private static UserRepository? _instance;

        public static UserRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserRepository();
                }

                return _instance;
            }
        }
        #endregion

        private readonly CardRepository _cardRepository = CardRepository.Instance;
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
                SELECT userId, username, password, authToken, coinCount, elopoints
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
                SELECT userId, username, password, authToken, coinCount, elopoints
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
                WHERE userId = @id
                """);

            DataLayer.AddParameterWithValue(dbCommand, "@username", DbType.String, user.Username);
            DataLayer.AddParameterWithValue(dbCommand, "@password", DbType.String, user.Password);
            DataLayer.AddParameterWithValue(dbCommand, "@authToken", DbType.String, user.AuthToken);
            DataLayer.AddParameterWithValue(dbCommand, "@coinCount", DbType.Int32, user.CoinCount);
            DataLayer.AddParameterWithValue(dbCommand, "@eloPoints", DbType.Int32, user.EloPoints);
            DataLayer.AddParameterWithValue(dbCommand, "@id", DbType.Int32, user.Id);

            dbCommand.ExecuteNonQuery();
        }

        public User? GetUserByToken(string token)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT userId, username, password, authToken, coinCount, elopoints
                                                                  FROM users
                                                                  WHERE authToken = @token
                                                                  """);
            DataLayer.AddParameterWithValue(dbCommand, "@token", DbType.String, token);

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

        public bool ClearUserStack(User user)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  DELETE FROM userCards
                                                                  WHERE userid = @userId;
                                                                  """);

            DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

            int rowsAffected = dbCommand.ExecuteNonQuery();

            if (rowsAffected <= 0)
            {
                Console.WriteLine($"[Error] Cards of User {user.Username} couldn't be removed from database!");
                return false;
            }

            return true;
        }

        public bool SaveStackOfUser(User user)
        {
            foreach (var card in user.Stack.Cards)
            {
                using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                      INSERT INTO userCards (userId, cardId)
                                                                      VALUES (@userId, @cardId);
                                                                      """);

                DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);
                DataLayer.AddParameterWithValue(dbCommand, "@cardId", DbType.String, card.Id);

                int rowsAffected = dbCommand.ExecuteNonQuery();

                if (rowsAffected <= 0)
                {
                    Console.WriteLine($"[Error] Card {card.Name} couldn't be added to database!");
                    return false;
                }
            }

            return true;
        }

        public List<string>? GetCardIdsOfUserStack(User user)
        {
            using IDbCommand dbCommand = _dataLayer.CreateCommand("""
                                                                  SELECT cardId FROM userCards
                                                                  WHERE userId = @userId
                                                                  """);

            DataLayer.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, user.Id);

            using IDataReader reader = dbCommand.ExecuteReader();

            List<string> cardsOfUser = new List<string>();

            while (reader.Read())
            {
                cardsOfUser.Add(reader.GetString(0));
            }

            return cardsOfUser;
        }
    }
}
