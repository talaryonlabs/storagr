using System;
using System.Collections.Generic;
using System.Linq;

namespace Storagr.Server.Data.Builders
{
    public class SqlFilterBuilder : IDatabaseFilter
    {
        public bool HasItems => _list?.Count > 0;

        private readonly List<string> _list;
        
        public SqlFilterBuilder()
        {
            _list = new List<string>();
        }

        private void CheckAndOr()
        {
            if (_list.Count % 2 == 0)
                throw new Exception("AND|OR already added before.");
        }

        private void CheckWhereStatement()
        {
            if (_list.Count % 2 != 0)
                throw new Exception("Add AND or OR before `` statement.");
        }
        
        public string Build()
        {
            return string.Join(' ', _list).Trim();
        }

        public IDatabaseFilter Clamp(Action<IDatabaseFilter> filterBuilder)
        {
            CheckWhereStatement();
            if (filterBuilder is null)
                return this;


            var builder = new SqlFilterBuilder();
            filterBuilder.Invoke(builder);
            _list.Add($"({builder.Build()})");

            return this;
        }

        public IDatabaseFilter Or()
        {
            CheckAndOr();
            _list.Add("OR");
            return this;
        }

        public IDatabaseFilter And()
        {
            CheckAndOr();
            _list.Add("AND");
            return this;
        }

        public IDatabaseFilter Not()
        {
            _list.Add("NOT");
            return this;
        }

        public IDatabaseFilter Equal(string column, string value)
        {
            CheckWhereStatement();
            _list.Add($"{column} = '{value}'");
            return this;
        }

        public IDatabaseFilter Like(string column, string pattern)
        {
            CheckWhereStatement();
            _list.Add($"{column} LIKE '{pattern}'");
            return this;
        }

        public IDatabaseFilter In(string column, IEnumerable<string> values)
        {
            CheckWhereStatement();
            _list.Add($"{column} IN ({string.Join(",", values.Select(v => $"'{v}'"))})");
            return this;
        }

        public IDatabaseFilter Between(string column, string value1, string value2)
        {
            CheckWhereStatement();
            _list.Add($"{column} BETWEEN '{value1}' AND '{value2}'");
            return this;
        }

        public IDatabaseFilter GreaterThan(string column, string value)
        {
            CheckWhereStatement();
            _list.Add($"{column} > '{value}'");
            return this;
        }

        public IDatabaseFilter GreaterThanOrEqual(string column, string value)
        {
            CheckWhereStatement();
            _list.Add($"{column} >= '{value}'");
            return this;
        }

        public IDatabaseFilter LessThan(string column, string value)
        {
            CheckWhereStatement();
            _list.Add($"{column} < '{value}'");
            return this;
        }

        public IDatabaseFilter LessThanOrEqual(string column, string value)
        {
            CheckWhereStatement();
            _list.Add($"{column} >= '{value}'");
            return this;
        }
    }
}