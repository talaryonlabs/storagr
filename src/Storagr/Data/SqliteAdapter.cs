using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Storagr.Data.Builders;
using Storagr.Shared;

namespace Storagr.Data
{
    [StoragrConfig("Sqlite")]
    public sealed class SqliteOptions : StoragrOptions<SqliteOptions>
    {
        [StoragrConfigValue] public string DataSource { get; set; }
    }

    public sealed class SqliteAdapter : SqlAdapterBase, IDisposable
    {
        private readonly IDbConnection _connection;

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