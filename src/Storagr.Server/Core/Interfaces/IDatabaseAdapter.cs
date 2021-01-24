using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Storagr.Server
{
    public interface IDatabaseAdapter
    {
        IDatabaseAdapter UseConnection(IDbConnection connection);
        
        Task<int> Count<T>(CancellationToken cancellationToken = default) where T : class => Count<T>(null, cancellationToken);
        Task<int> Count<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken = default) where T : class;

        Task<bool> Exists<T>(string id, CancellationToken cancellationToken = default) where T : class;
        Task<bool> Exists<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken = default) where T : class;

        Task<T> Get<T>(string id, CancellationToken cancellationToken = default) where T : class;
        Task<T> Get<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken = default) where T : class;
        Task<T> Get<T>(Action<IDatabaseQuery> queryBuilder, CancellationToken cancellationToken = default) where T : class;
        Task<IEnumerable<T>> GetMany<T>(Action<IDatabaseQuery> queryBuilder, CancellationToken cancellationToken = default) where T : class;
        Task<IEnumerable<T>> GetMany<T>(Action<IDatabaseFilter> filterBuilder, CancellationToken cancellationToken = default) where T : class;
        Task<IEnumerable<T>> GetAll<T>(CancellationToken cancellationToken = default) where T : class;

        Task<int> Insert<T>(T data, CancellationToken cancellationToken = default) where T : class;
        Task Update<T>(T data, CancellationToken cancellationToken = default) where T : class;

        Task<bool> Delete<T>(T data, CancellationToken cancellationToken = default) where T : class;
        Task<bool> Delete<T>(IEnumerable<T> list, CancellationToken cancellationToken = default) where T : class;
    }

    public interface IDatabaseQuery
    {
        IDatabaseQuery Select(params string[] columns);
        IDatabaseQuery Distinct();
        IDatabaseQuery Where(Action<IDatabaseFilter> filterBuilder);
        IDatabaseQuery Limit(int limit);
        IDatabaseQuery Offset(int offset);
        IDatabaseQuery OrderBy(Action<IDatabaseOrder> orderByBuilder);
    }

    public enum DatabaseOrderType
    {
        /// <summary>
        /// Ascending Order
        /// </summary>
        Asc,
        
        /// <summary>
        /// Descending Order
        /// </summary>
        Desc
    }
    
    public interface IDatabaseOrder
    {
        IDatabaseOrder Column(string name) => Column(name, DatabaseOrderType.Asc);
        IDatabaseOrder Column(string name, DatabaseOrderType type);
    }
    
    public interface IDatabaseFilter
    {
        bool HasItems { get; }
        
        IDatabaseFilter Clamp(Action<IDatabaseFilter> filterBuilder);

        IDatabaseFilter Or();
        IDatabaseFilter And();
        IDatabaseFilter Not();
        
        IDatabaseFilter Equal(string column, string value);
        IDatabaseFilter Like(string column, string pattern);
        IDatabaseFilter In(string column, IEnumerable<string> values);
        IDatabaseFilter Between(string column, string value1, string value2);

        IDatabaseFilter GreaterThan(string column, string value);
        IDatabaseFilter GreaterThan(string column, int value) => GreaterThan(column, value.ToString());
        IDatabaseFilter GreaterThanOrEqual(string column, string value);
        IDatabaseFilter GreaterThanOrEqual(string column, int value) => GreaterThan(column, value.ToString());
        
        IDatabaseFilter LessThan(string column, string value);
        IDatabaseFilter LessThan(string column, int value) => GreaterThan(column, value.ToString());
        IDatabaseFilter LessThanOrEqual(string column, string value);
        IDatabaseFilter LessThanOrEqual(string column, int value) => GreaterThan(column, value.ToString());
    }
}