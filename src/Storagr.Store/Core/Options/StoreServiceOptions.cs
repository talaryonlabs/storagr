using System;
using Storagr.Shared;

namespace Storagr.Store
{
    public class StoreServiceOptions : StoragrOptions<StoreServiceOptions>
    {
        public string RootPath { get; set; }
        public int BufferSize { get; set; }
        public TimeSpan Expiration { get; set; }
        public TimeSpan ScanInterval { get; set; }
    }
}