using System.Collections.Generic;
using Storagr.Server.Data.Entities;

namespace Storagr.Server
{
    public interface IBatchParams
    {
        IBatchParams Transfers(IEnumerable<string> transfers);
        IBatchParams Ref(string name);
    }
}