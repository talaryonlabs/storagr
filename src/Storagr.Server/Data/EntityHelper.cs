using System;
using System.Linq;
using System.Reflection;
using Dapper.Contrib.Extensions;

namespace Storagr.Server.Data
{
    public static class EntityHelper
    {
        public static string GetTableName<T>()
        {
            if ((TableAttribute) typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() is null)
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
                    
            return typeof(T)
                .GetCustomAttribute<TableAttribute>()?
                .Name;
        }
        
        public static string GetKeyValue<T>(T entity)
        {
            return (string)typeof(T)
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() is not null)?
                .GetValue(entity);
        }
        
        public static string GetKeyName<T>()
        {
            return (string)typeof(T)
                .GetProperties()
                .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() is not null)?
                .Name;
        }
    }
}