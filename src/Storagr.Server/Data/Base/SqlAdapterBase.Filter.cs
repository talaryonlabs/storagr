using System;
using System.Collections.Generic;
using System.Linq;

namespace Storagr.Server.Data
{
    public partial class SqlAdapterBase
    {
        private class SqlQueryFilter<T> :
        IDatabaseFilter<T>,
        IDatabaseFilterColumn<T>,
        IDatabaseFilterOperator<T>
    {
        private readonly List<string> _filterParams;

        public SqlQueryFilter()
        {
            _filterParams = new List<string>();
        }

        public string BuildFilter()
        {
            if (!_filterParams.Any())
                return "1";
            
            if (_filterParams[^1] == "AND" || _filterParams[^1] == "OR" || _filterParams[^1] == "NOT")
                _filterParams.RemoveAt(_filterParams.Count - 1);
            
            return string.Join(" ", _filterParams);
        }
        
        IDatabaseFilterColumn<T> IDatabaseFilter<T>.Is<TJoinItem>(string column)
        {
            _filterParams
                .Add($"{EntityHelper.GetTableName<TJoinItem>()}.{column}");
            
            return this;
        }

        IDatabaseFilterColumn<T> IDatabaseFilter<T>.IsNot<TJoinItem>(string column)
        {
            _filterParams
                .Add($"NOT {EntityHelper.GetTableName<TJoinItem>()}.{column}");
            
            return this;
        }

        IDatabaseFilterOperator<T> IDatabaseFilter<T>.Clamp(Action<IDatabaseFilter<T>> filter)
        {
            var f = new SqlQueryFilter<T>();
            filter(f);
            _filterParams.Add($"$({f.BuildFilter()})");
            return this;
        }

        IDatabaseFilterOperator<T> IDatabaseFilterColumn<T>.EqualTo(object value)
        {
            _filterParams.Add($"= '{value}'");
            return this;
        }

        IDatabaseFilterOperator<T> IDatabaseFilterColumn<T>.Like(object value, DatabasePatternType patternType)
        {
            if (patternType.HasFlag(DatabasePatternType.EndsWith))
                _filterParams.Add($"LIKE '%{value}'");
            else if (patternType.HasFlag(DatabasePatternType.StartsWith))
                _filterParams.Add($"LIKE '{value}%'");
                
            _filterParams.Add($"LIKE '%{value}%'");
            return this;
        }

        IDatabaseFilterOperator<T> IDatabaseFilterColumn<T>.In(IEnumerable<object> values)
        {
            _filterParams.Add($"IN ({string.Join(",", values.Select(v => $"'{v}'"))})");
            return this;
        }

        IDatabaseFilterOperator<T> IDatabaseFilterColumn<T>.Between(object value1, object value2)
        {
            _filterParams.Add($"BETWEEN '{value1}' AND '{value2}'");
            return this;
        }

        IDatabaseFilter<T> IDatabaseFilterOperator<T>.Or()
        {
            _filterParams.Add("OR");
            return this;
        }

        IDatabaseFilter<T> IDatabaseFilterOperator<T>.And()
        {
            _filterParams.Add("AND");
            return this;
        }
    }
    }
}