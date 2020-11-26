using System;
using Storagr.Shared;

namespace Storagr.Store
{
    [StoragrConfig]
    public class StoreServiceOptions : StoragrOptions<StoreServiceOptions>
    {
        [StoragrConfigValue] public string RootPath { get; set; }
        [StoragrConfigValue] public int BufferSize { get; set; }
        [StoragrConfigValue] public int Expiration { get; set; }
        [StoragrConfigValue] public int ScanInterval { get; set; }
    }
}