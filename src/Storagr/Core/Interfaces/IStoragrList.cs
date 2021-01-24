using System.Collections.Generic;

namespace Storagr
{
    public interface IStoragrList<TItem>
    {
        IEnumerable<TItem> Items { get; set; }
        int TotalCount { get; set; }
        string NextCursor { get; set; }
    }
}