namespace WebApplicationServerAndClient.Repository
{
    using Models;
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using WebApplicationServerAndClient.Interfaces;

    namespace MessageService.Data
    {
        public class MessageRepository : IMessageRepository
        {
            private readonly string _connectionString;
            private readonly ILogger<MessageRepository> _logger;

            public MessageRepository(IConfiguration configuration, ILogger<MessageRepository> logger)
            {
                _connectionString = configuration.GetConnectionString("DefaultConnecting");
                _logger = logger;
            }

            private async Task<NpgsqlConnection> GetConnectionAsync()
            {
                var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection;
            }

            private async Task<object?> ExecuteCommandAsync(string sql, Dictionary<string, object>? parameters = null, bool returnScalar = false)
            {
                await using var connection = await GetConnectionAsync();
                await using var command = new NpgsqlCommand(sql, connection);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                return returnScalar ? await command.ExecuteScalarAsync() : await command.ExecuteNonQueryAsync();
            }
            private async Task<List<T>> ExecuteReaderAsync<T>(string sql, Dictionary<string, object>? parameters, Func<NpgsqlDataReader, T> mapFunction)
            {
                var result = new List<T>();

                await using var connection = await GetConnectionAsync();
                await using var command = new NpgsqlCommand(sql, connection);

                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.AddWithValue(param.Key, param.Value);
                    }
                }

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(mapFunction(reader));
                }

                return result;
            }

            public async Task InitializeDatabaseAsync()
            {
                string sql = @"
                CREATE TABLE IF NOT EXISTS messages (
                id SERIAL PRIMARY KEY,
                content TEXT NOT NULL,
                timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";

                await ExecuteCommandAsync(sql);
            }

            public async Task SaveMessageAsync(Message message)
            {

                if (message.Content.Length > 128)
                {
                    _logger.LogWarning("Попытка сохранить сообщение с длиной более 128 символов: {ContentLength}", message.Content.Length);
                    throw new ArgumentException("Сообщение не может быть длиннее 128 символов.");
                }
                string sql = "INSERT INTO messages (content, timestamp) VALUES (@content, @timestamp) RETURNING id;";
                var parameters = new Dictionary<string, object>
                {
                    { "@content", message.Content },
                    { "@timestamp", message.Timestamp }
                };

                object? result = await ExecuteCommandAsync(sql, parameters, returnScalar: true);
                message.Id = Convert.ToInt32(result);
            }

            public async Task<List<Message>> GetMessagesAsync(DateTime from, DateTime to)
            {
                string sql = "SELECT id, content, timestamp FROM messages WHERE timestamp BETWEEN @from AND @to ORDER BY timestamp;";
                var parameters = new Dictionary<string, object>
                {
                    { "@from", from },
                    { "@to", to }
                };

                return await ExecuteReaderAsync(sql, parameters, reader => new Message
                {
                    Id = reader.GetInt32(0),
                    Content = reader.GetString(1),
                    Timestamp = reader.GetDateTime(2)
                });
            }
        }
    }

}
