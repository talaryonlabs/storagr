using System;
using System.Collections.Generic;

namespace Storagr.Data.Builders
{
    public class SqlQueryBuilder : IDatabaseQuery
    {
        private string _selector;
            private bool _distinct;
            private int _limit, _offset;
            private string _orderBy;

            private SqlFilterBuilder _filter;
            
            public SqlQueryBuilder()
            {
                _selector = "*";
                _limit = -1;
                _offset = -1;
                _filter = new SqlFilterBuilder();
            }

            public string Build(string table)
            {
                var query = new List<string>(new[] {"SELECT", _selector, "FROM", table});
                
                if(_distinct)
                    query.Insert(1, "DISTINCT");

                if (_filter.HasItems)
                    query.AddRange(new[] {"WHERE", _filter.Build()});

                if (!string.IsNullOrEmpty(_orderBy))
                    query.AddRange(new []{"ORDER BY", _orderBy});
                
                if (_limit > 0)
                    query.AddRange(new[] {"LIMIT", _limit.ToString()});

                if (_offset > 0)
                    query.AddRange(new[] {"OFFSET", _offset.ToString()});

                return string.Join(" ", query);
            }
            
            public IDatabaseQuery Select(params string[] columns)
            {
                _selector = columns.Length == 0 ? "*" : string.Join(", ", columns);
                return this;
            }

            public IDatabaseQuery Distinct()
            {
                _distinct = true;
                return this;
            }

            public IDatabaseQuery Where(Action<IDatabaseFilter> filterBuilder)
            {
                _filter = new SqlFilterBuilder();
                
                filterBuilder?.Invoke(_filter);

                return this;
            }

            public IDatabaseQuery Limit(int limit)
            {
                _limit = limit;
                return this;
            }

            public IDatabaseQuery Offset(int offset)
            {
                _offset = offset;
                return this;
            }

            public IDatabaseQuery OrderBy(Action<IDatabaseOrder> orderByBuilder)
            {
                var builder = new SqlOrderBuilder();
                
                orderByBuilder?.Invoke(builder);

                _orderBy = builder.Build();
                return this;
            }
    }
}