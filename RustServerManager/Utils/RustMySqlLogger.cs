using MySql.Data.MySqlClient;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;

namespace RustServerManager.Utils
{
    public class RustMySqlLogger
    {
        private readonly string _connectionString;
        private readonly string _serverIdentity;

        public RustMySqlLogger(string serverIdentity)
        {
            _serverIdentity = serverIdentity;
            _connectionString = DatabaseHelper.GetConnectionString();
        }

        public async Task LogAsync(string logType, string errorType, string severity, string source, string message, string details)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO rust_log (ServerIdentity, LogType, ErrorType, Severity, Source, Message, Details, Timestamp)
                VALUES (@identity, @logType, @errorType, @severity, @source, @message, @details, @timestamp)";
            cmd.Parameters.AddWithValue("@identity", _serverIdentity);
            cmd.Parameters.AddWithValue("@logType", logType);
            cmd.Parameters.AddWithValue("@errorType", errorType);
            cmd.Parameters.AddWithValue("@severity", severity);
            cmd.Parameters.AddWithValue("@source", source);
            cmd.Parameters.AddWithValue("@message", message);
            cmd.Parameters.AddWithValue("@details", details);
            cmd.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync();
        }
    }
}
