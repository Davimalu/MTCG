using MTCG.DAL;
using MTCG.Interfaces.Logic;
using MTCG.Interfaces.Repository;
using MTCG.Logic;
using MTCG.Models;
using MTCG.Models.Enums;
using System.Data;

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
        private readonly DatabaseService _databaseService = DatabaseService.Instance;
        private readonly IEventService _eventService = new EventService();

        /// <summary>
        /// saves a new user to the database
        /// </summary>
        /// <param name="user">user object containing all user data, Stack and Deck are optional/ignored</param>
        /// <returns>
        /// <para>ID of the newly created database entry on success</para>
        /// <para>null on error</para>
        /// </returns>
        public int? SaveUserToDatabase(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                INSERT INTO users (username, chosenName, biography, image, password, authToken, coinCount, wins, losses, ties, eloPoints)
                VALUES (@username, @chosenName, @biography, @image, @password, @authToken, @coinCount, @wins, @losses, @ties, @eloPoints)
                RETURNING userId;
                """);

                DatabaseService.AddParameterWithValue(dbCommand, "@username", DbType.String, user.Username);
                DatabaseService.AddParameterWithValue(dbCommand, "@chosenName", DbType.String, user.DisplayName);
                DatabaseService.AddParameterWithValue(dbCommand, "@biography", DbType.String, user.Biography);
                DatabaseService.AddParameterWithValue(dbCommand, "@image", DbType.String, user.Image);
                DatabaseService.AddParameterWithValue(dbCommand, "@password", DbType.String, user.Password);
                DatabaseService.AddParameterWithValue(dbCommand, "@authToken", DbType.String, user.AuthToken);
                DatabaseService.AddParameterWithValue(dbCommand, "@coinCount", DbType.Int32, user.CoinCount);
                DatabaseService.AddParameterWithValue(dbCommand, "@wins", DbType.Int32, user.Stats.Wins);
                DatabaseService.AddParameterWithValue(dbCommand, "@losses", DbType.Int32, user.Stats.Losses);
                DatabaseService.AddParameterWithValue(dbCommand, "@ties", DbType.Int32, user.Stats.Ties);
                DatabaseService.AddParameterWithValue(dbCommand, "@eloPoints", DbType.Int32, user.Stats.EloPoints);

                // Execute query and error handling
                try
                {
                    user.Id = (int)(dbCommand.ExecuteScalar() ?? 0);
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"User {user.Username} couldn't be added to the database!", ex);
                    return null;
                }

                _eventService.LogEvent(EventType.Highlight, $"User {user.Username} added to database!", null);
                return user.Id;
            }
        }

        /// <summary>
        /// retrieve a user from the database using his name
        /// </summary>
        /// <param name="username"></param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that name or an error occured</para>
        /// </returns>
        public User? GetUserByName(string username)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT userId, username, chosenName, biography, image, password, authToken, coinCount, wins, losses, ties, eloPoints
                                                                      FROM users
                                                                      WHERE username = @username
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@username", DbType.String, username);

                // Execute query and Error Handling
                try
                {
                    using IDataReader reader = dbCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        UserStatistics stats = new UserStatistics()
                        {
                            Wins = reader.GetInt32(8),
                            Losses = reader.GetInt32(9),
                            Ties = reader.GetInt32(10),
                            EloPoints = reader.GetInt32(11)
                        };

                        return new User()
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            DisplayName = reader.GetString(2),
                            Biography = reader.GetString(3),
                            Image = reader.GetString(4),
                            Password = reader.GetString(5),
                            AuthToken = reader.GetString(6),
                            CoinCount = reader.GetInt32(7),
                            Stats = stats
                        };
                    }
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Error retrieving {username} from the database!", ex);
                    return null;
                }

                // If no entries were returned...
                return null;
            }
        }

        /// <summary>
        /// update the information of an existing user in the users table; does NOT update the users stack or deck
        /// </summary>
        /// <param name="user">user object containing the updated user data, Stack and Deck are optional/ignored</param>
        /// <returns>
        /// <para>ID of the updated database entry on success</para>
        /// <para>null if the user has not yet been added to the database or on error</para>
        /// </returns>
        public int? UpdateUser(User user)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL Query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                UPDATE users
                SET username = @username, chosenName = @chosenName, biography = @biography, image = @image, password = @password, authToken = @authToken, coinCount = @coinCount, wins = @wins, losses = @losses, ties = @ties, eloPoints = @eloPoints
                WHERE userId = @id
                RETURNING userId
                """);

                DatabaseService.AddParameterWithValue(dbCommand, "@username", DbType.String, user.Username);
                DatabaseService.AddParameterWithValue(dbCommand, "@chosenName", DbType.String, user.DisplayName);
                DatabaseService.AddParameterWithValue(dbCommand, "@biography", DbType.String, user.Biography);
                DatabaseService.AddParameterWithValue(dbCommand, "@image", DbType.String, user.Image);
                DatabaseService.AddParameterWithValue(dbCommand, "@password", DbType.String, user.Password);
                DatabaseService.AddParameterWithValue(dbCommand, "@authToken", DbType.String, user.AuthToken);
                DatabaseService.AddParameterWithValue(dbCommand, "@coinCount", DbType.Int32, user.CoinCount);
                DatabaseService.AddParameterWithValue(dbCommand, "@wins", DbType.Int32, user.Stats.Wins);
                DatabaseService.AddParameterWithValue(dbCommand, "@losses", DbType.Int32, user.Stats.Losses);
                DatabaseService.AddParameterWithValue(dbCommand, "@ties", DbType.Int32, user.Stats.Ties);
                DatabaseService.AddParameterWithValue(dbCommand, "@eloPoints", DbType.Int32, user.Stats.EloPoints);
                DatabaseService.AddParameterWithValue(dbCommand, "@id", DbType.Int32, user.Id);

                // Execute query and error handling
                int userId;
                try
                {
                    userId = (int)(dbCommand.ExecuteScalar() ?? 0);
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Changes for user {user.Username} couldn't be written to the database!", ex);
                    return null;
                }

                return userId;
            }
        }

        /// <summary>
        /// retrieve a user from the database using his token
        /// </summary>
        /// <param name="token">the authentication token in format "xxx-mtcgToken"</param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that token or an error occured</para>
        /// </returns>
        public User? GetUserByToken(string token)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT userId, username, chosenName, biography, image, password, authToken, coinCount, wins, losses, ties, eloPoints
                                                                      FROM users
                                                                      WHERE authToken = @token
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@token", DbType.String, token);

                // Execute query and error handling
                try
                {
                    using IDataReader reader = dbCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        UserStatistics stats = new UserStatistics()
                        {
                            Wins = reader.GetInt32(8),
                            Losses = reader.GetInt32(9),
                            Ties = reader.GetInt32(10),
                            EloPoints = reader.GetInt32(11)
                        };

                        return new User()
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            DisplayName = reader.GetString(2),
                            Biography = reader.GetString(3),
                            Image = reader.GetString(4),
                            Password = reader.GetString(5),
                            AuthToken = reader.GetString(6),
                            CoinCount = reader.GetInt32(7),
                            Stats = stats
                        };
                    }
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Error retrieving user with token {token} from the database!", ex);
                    return null;
                }

                // If no entries were returned...
                return null;
            }
        }

        /// <summary>
        /// retrieve a user from the database using his Id
        /// </summary>
        /// <param name="userId">the unique id of the user</param>
        /// <returns>
        /// <para>user object containing all information from the users table on success (NO Stack/Deck)</para>
        /// <para>null if there's no user with that id or an error occured</para>
        /// </returns>
        public User? GetUserById(int userId)
        {
            lock (ThreadSync.DatabaseLock)
            {
                // Prepare SQL query
                using IDbCommand dbCommand = _databaseService.CreateCommand("""
                                                                      SELECT userId, username, chosenName, biography, image, password, authToken, coinCount, wins, losses, ties, eloPoints
                                                                      FROM users
                                                                      WHERE userId = @userId
                                                                      """);

                DatabaseService.AddParameterWithValue(dbCommand, "@userId", DbType.Int32, userId);

                // Execute query and error handling
                try
                {
                    using IDataReader reader = dbCommand.ExecuteReader();

                    if (reader.Read())
                    {
                        UserStatistics stats = new UserStatistics()
                        {
                            Wins = reader.GetInt32(8),
                            Losses = reader.GetInt32(9),
                            Ties = reader.GetInt32(10),
                            EloPoints = reader.GetInt32(11)
                        };

                        return new User()
                        {
                            Id = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            DisplayName = reader.GetString(2),
                            Biography = reader.GetString(3),
                            Image = reader.GetString(4),
                            Password = reader.GetString(5),
                            AuthToken = reader.GetString(6),
                            CoinCount = reader.GetInt32(7),
                            Stats = stats
                        };
                    }
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Error retrieving user with ID {userId} from the database!", ex);
                    return null;
                }

                // If no entries were returned...
                return null;
            }
        }

        /// <summary>
        /// returns a list of usernames of all users currently in the database
        /// </summary>
        /// <returns>
        /// <para>A list of strings containing the usernames (Primary Key) of each user</para>
        /// </returns>
        public List<string> GetListOfUsers()
        {
            lock (ThreadSync.DatabaseLock)
            {
                List<string> users = new List<string>();

                // Prepare SQL statement
                using IDbCommand dbCommand = _databaseService.CreateCommand("SELECT username FROM users");

                // Execute query
                try
                {
                    using IDataReader reader = dbCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        users.Add(reader.GetString(0));
                    }
                }
                catch (Exception ex)
                {
                    _eventService.LogEvent(EventType.Error, $"Error retrieving list of all users from the database!", ex);
                    return users;
                }

                return users;
            }
        }
    }
}
