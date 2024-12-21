using MTCG.Interfaces.Logic;
using MTCG.Logic;
using MTCG.Models.Enums;
using Npgsql;
using System.Data;

namespace MTCG.DAL
{
    public class DatabaseService : IDisposable
    {
        #region Singleton
        private static DatabaseService? _instance;
        public static DatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseService("Host=localhost;Database=mtcg;Username=swen1;Password=passwordswen1;Persist Security Info=True; Include Error Detail=True");
                }
                return _instance;
            }
        }
        #endregion

        private readonly string _connectionString;
        private IDbConnection? _connection;
        private readonly IEventService _eventService = new EventService();


        public DatabaseService(string connectionString)
        {
            this._connectionString = connectionString;
        }


        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }


        public bool ConnectToDatabase()
        {
            try
            {
                _connection = new NpgsqlConnection(_connectionString);
                _connection.Open();
            }
            catch (Exception ex)
            {
                _eventService.LogEvent(EventType.Error, $"Error establishing connection to the Postgres Database", ex);
                return false;
            }

            _eventService.LogEvent(EventType.Info, $"Connection to Postgres Database established", null);
            return true;
        }


        public IDbCommand CreateCommand(string commandText)
        {
            if (_connection == null)
            {
                _eventService.LogEvent(EventType.Error, $"Connection to database lost", null);
                Environment.Exit(-1);
            }

            IDbCommand command = _connection.CreateCommand();
            command.CommandText = commandText;

            return command;
        }


        public static void AddParameterWithValue(IDbCommand command, string parameterName, DbType type, object? value)
        {
            IDbDataParameter parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.DbType = type;
            parameter.Value = value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
