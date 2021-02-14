using System.Collections.Generic;

namespace Storagr.Shared
{
    public interface IStoragrList<TItem>
    {
        IEnumerable<TItem> Items { get; set; }
        int TotalCount { get; set; }
        string NextCursor { get; set; }
    }
}