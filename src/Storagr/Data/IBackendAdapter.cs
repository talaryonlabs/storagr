using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Storagr.Data
{
    public interface IBackendAdapter
    {
        Task<T> Get<T>(string id) where T : class;
        Task<T> Get<T>(Action<IBackendQuery> queryBuilder) where T : class;
        
        Task<IEnumerable<T>> GetAll<T>() where T : class;
        Task<IEnumerable<T>> GetAll<T>(Action<IBackendQuery> queryBuilder) where T : class;

        Task<int> Insert<T>(T data) where T : class;
        Task Update<T>(T data) where T : class;
        
        Task<bool> Delete<T>(T data) where T : class;
    }

    public interface IBackendQuery
    {
        IBackendQuery Select(params string[] columns);
        IBackendQuery Distinct();
        IBackendQuery Where(Action<IBackendFilter> filterBuilder);
        IBackendQuery Limit(int limit);
        IBackendQuery Offset(int offset);
        IBackendQuery OrderBy(Action<IBackendOrder> orderByBuilder);
    }

    public enum BackendOrderType
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
    
    public interface IBackendOrder
    {
        IBackendOrder Column(string name) => Column(name, BackendOrderType.Asc);
        IBackendOrder Column(string name, BackendOrderType type);
    }
    
    public interface IBackendFilter
    {
        IBackendFilter Clamp(Action<IBackendFilter> filterBuilder);

        IBackendFilter Or();
        IBackendFilter And();
        IBackendFilter Not();
        
        IBackendFilter Equal(string column, string value);
        IBackendFilter Like(string column, string pattern);
        IBackendFilter In(string column, params string[] values);
        IBackendFilter Between(string column, string value1, string value2);

        IBackendFilter GreaterThan(string column, string value);
        IBackendFilter GreaterThan(string column, int value) => GreaterThan(column, value.ToString());
        IBackendFilter GreaterThanOrEqual(string column, string value);
        IBackendFilter GreaterThanOrEqual(string column, int value) => GreaterThan(column, value.ToString());
        
        IBackendFilter LessThan(string column, string value);
        IBackendFilter LessThan(string column, int value) => GreaterThan(column, value.ToString());
        IBackendFilter LessThanOrEqual(string column, string value);
        IBackendFilter LessThanOrEqual(string column, int value) => GreaterThan(column, value.ToString());
    }
}