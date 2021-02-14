using System;
using System.Collections.Generic;

namespace Storagr.Server.Data
{
    public partial class SqlAdapterBase
    {
        private class QueryBuilder<TItem>
        {
            private readonly Dictionary<string, object> _queryParams;
            private readonly string _table;


            public QueryBuilder()
            {
                _queryParams = new Dictionary<string, object>();
                _table = EntityHelper.GetTableName<TItem>();
            }

            public void Select(string column, string alias = null) => Select<TItem>(column, alias);

            public void Select<TJoinItem>(string column, string alias = null)
            {
                if (!_queryParams.ContainsKey("columns"))
                    _queryParams.Add("columns", new List<string>());
 
                ((List<string>) _queryParams["columns"])
                    .Add(
                        alias is not null
                            ? $"{EntityHelper.GetTableName<TJoinItem>()}.{column} as {alias}"
                            : $"{EntityHelper.GetTableName<TJoinItem>()}.{column}"
                    );
            }

            public void Distinct()
            {
                if (!_queryParams.ContainsKey("distinct"))
                    _queryParams.Add("distinct", null);
            }

            public void Join<TJoinItem>(string column, string joinedColumn)
            {
                if (!_queryParams.ContainsKey("joins"))
                    _queryParams.Add("joins", new List<string>());

                var joinedTable = EntityHelper.GetTableName<TJoinItem>();
                ((List<string>) _queryParams["joins"])
                    .Add($"INNER JOIN {joinedTable} ON {joinedTable}.{joinedColumn} = {_table}.{column}");
            }

            public void Where(Action<IDatabaseFilter<TItem>> filter)
            {
                var builder = new SqlQueryFilter<TItem>();
                filter(builder);

                if (!_queryParams.ContainsKey("where"))
                    _queryParams.Add("where", builder.BuildFilter());
            }

            public void Limit(int count)
            {
                if (!_queryParams.ContainsKey("limit"))
                    _queryParams.Add("limit", count.ToString());
            }

            public void Offset(int count)
            {
                if (!_queryParams.ContainsKey("offset"))
                    _queryParams.Add("offset", count.ToString());
            }

            public void Order<TJoinItem>(string column, bool asc = true)
            {
                if (!_queryParams.ContainsKey("orders"))
                    _queryParams.Add("orders", new List<string>());

                ((List<string>) _queryParams["orders"])
                    .Add($"{EntityHelper.GetTableName<TJoinItem>()}.{column} {(asc ? "ASC" : "DESC")}");
            }

            public string Build()
            {
                var query = new List<string>();

                query.AddRange(new[]
                {
                    (
                        _queryParams.ContainsKey("distinct")
                            ? "SELECT DISTINCT"
                            : "SELECT"
                    ),
                    (
                        _queryParams.ContainsKey("columns")
                            ? string.Join(", ", (List<string>) _queryParams["columns"])
                            : "*"
                    ),
                    "FROM",
                    _table
                });

                if (_queryParams.ContainsKey("joins"))
                    query.Add((string) _queryParams["joins"]);

                if (_queryParams.ContainsKey("where"))
                    query.AddRange(new[]
                    {
                        "WHERE",
                        (string) _queryParams["where"]
                    });

                if (_queryParams.ContainsKey("orders"))
                    query.AddRange(new[]
                    {
                        "ORDER BY",
                        string.Join(", ", (List<string>) _queryParams["orders"])
                    });


                if (_queryParams.ContainsKey("limit"))
                    query.AddRange(new[]
                    {
                        "LIMIT",
                        (string) _queryParams["limit"]
                    });

                if (_queryParams.ContainsKey("offset"))
                    query.AddRange(new[]
                    {
                        "OFFSET",
                        (string) _queryParams["offset"]
                    });

                return string.Join(" ", _queryParams);
            }
        }
    }
}