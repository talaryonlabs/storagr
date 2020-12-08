using System.Collections.Generic;

namespace Storagr.Shared
{
    public interface IStoragrList<TItem> : IStoragrList<TItem, string>
    {
        
    }
    
    public interface IStoragrList<TItem, TCursor> : IEnumerable<TItem>
    {
        IEnumerable<TItem> Items { get; set; }
        TCursor NextCursor { get; set; }
        int TotalCount { get; set; }
    }

    public interface IStoragrListQuery : IStoragrListQuery<string>
    {
    
    }
    
    public interface IStoragrListQuery<TCursor>
    {
        TCursor Cursor { get; set; }
        int Limit { get; set; }
    }
}