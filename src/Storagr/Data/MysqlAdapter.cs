using System;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Storagr.Shared;

namespace Storagr.Data
{
    [StoragrConfig("MySql")]
    public sealed class MysqlOptions : StoragrOptions<MysqlOptions>
    {
        [StoragrConfigValue] public string Server { get; set; }
        [StoragrConfigValue] public string User { get; set; }
        [StoragrConfigValue] public string Password { get; set; }
        [StoragrConfigValue] public string Database { get; set; }
    }
    
    public sealed class MysqlAdapter : SqlAdapterBase, IDisposable
    {
        private readonly MySqlConnection _connection;

        public MysqlAdapter(IOptions<MysqlOptions> optionsAccessor)
        {
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            var options = optionsAccessor.Value;

            _connection = new MySqlConnection(
                $"server={options.Server};" +
                $"uid={options.User};" +
                $"pwd={options.Password};" +
                $"database={options.Database}"
            );

            UseConnection(_connection);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}