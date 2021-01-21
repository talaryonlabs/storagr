using System.Collections.Generic;

namespace Storagr.Data.Builders
{
    public class SqlOrderBuilder : IDatabaseOrder
    {
        private readonly List<string> _list;

        public SqlOrderBuilder()
        {
            _list = new List<string>();
        }
            
        public string Build()
        {
            return string.Join(", ", _list).Trim();
        }
            
        public IDatabaseOrder Column(string name, DatabaseOrderType type)
        {
            _list.Add($"{name} {(type == DatabaseOrderType.Asc ? "ASC" : "DESC")}");
            return this;
        }
    }
}