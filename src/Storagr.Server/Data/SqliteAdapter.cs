using System;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Storagr.Server.Data
{
    [StoragrConfig("Sqlite")]
    public sealed class SqliteOptions : StoragrOptions<SqliteOptions>
    {
        [StoragrConfigValue] public string DataSource { get; set; }
    }

    public sealed class SqliteAdapter : SqlAdapterBase, IDisposable
    {
        private readonly SqliteConnection _connection;

        public SqliteAdapter(IOptions<SqliteOptions> optionsAccessor)
        {
            if (optionsAccessor is null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            _connection = new SqliteConnection($"Data Source={optionsAccessor.Value.DataSource}");

            UseConnection(_connection);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}