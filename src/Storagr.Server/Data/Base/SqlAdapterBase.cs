using System;
using System.Collections.Generic;
using System.Data;
using Storagr.Shared;

namespace Storagr.Server.Data
{
    // SELECT [DISTINCT] [*]
    // FROM {table}
    // [JOIN {table2} ON {table}.{column} = {table2}.{column}]
    // [WHERE {table}.{column} = '{value'} AND ...]
    // [ORDER BY {table}.{column} [ASC|DESC]]
    // [LIMIT {limit}]
    // [OFFSET {offset}]
    public abstract partial class SqlAdapterBase : 
        IDatabaseAdapter
    {
        private IDbConnection _connection;

        protected void UseConnection(IDbConnection connection)
        {
            _connection = connection;
        }
    
        IDatabaseCount<TItem> IDatabaseAdapter.Count<TItem>() where TItem : class =>
            new Count<TItem>(_connection);

        IStoragrRunner<TItem> IDatabaseAdapter.Get<TItem>(string id) where TItem : class =>
            (new Query<TItem>(_connection) as IDatabaseQuery<TItem>)
                .Where(filter => filter
                    .Is(EntityHelper.GetKeyName<TItem>())
                    .EqualTo(id)
                );

        IDatabaseQuery<TItem> IDatabaseAdapter.First<TItem>() where TItem : class => 
            new Query<TItem>(_connection);
        

        IDatabaseQueryList<TItem> IDatabaseAdapter.Many<TItem>() where TItem : class =>
            new Query<TItem>(_connection);

        IStoragrRunner<TItem> IDatabaseAdapter.Insert<TItem>(TItem item) where TItem : class =>
            new Insert<TItem>(_connection, item);

        IStoragrRunner<TItem> IDatabaseAdapter.Update<TItem>(TItem item) where TItem : class =>
            new Update<TItem>(_connection, item);

        IStoragrRunner<TItem> IDatabaseAdapter.Delete<TItem>(TItem item) where TItem : class =>
            new Delete<TItem>(_connection, item);

        
        IStoragrRunner<IEnumerable<TItem>> IDatabaseAdapter.DeleteMany<TItem>(IEnumerable<TItem> items) where TItem : class
        {
            throw new NotImplementedException();
        }

        IDatabaseQuery<TItem> IDatabaseAdapter.Delete<TItem>() where TItem : class
        {
            throw new NotImplementedException();
        }

        IDatabaseQuery<IEnumerable<TItem>> IDatabaseAdapter.DeleteMany<TItem>() where TItem : class
        {
            throw new NotImplementedException();
        }
    }
}