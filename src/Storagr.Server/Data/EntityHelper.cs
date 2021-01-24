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
            CheckTableAttribute<T>();
            return typeof(T).GetCustomAttribute<TableAttribute>()?.Name;
        }

        public static void CheckTableAttribute<T>()
        {
            if ((TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute)).FirstOrDefault() is null)
                throw new Exception($"Type {typeof(T).Name} has no [Table] attribute.");
        }
    }
}