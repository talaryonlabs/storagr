using System.Collections.Generic;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Server
{
    public interface IBatchService
    {
        IBatchServiceRepository Repository(string repositoryIdOrName);
    }

    public interface IBatchServiceRepository
    {
        IBatchServiceObjects Objects(IEnumerable<StoragrObject> objects);
    }

    public interface IBatchServiceObjects
    {
        IBatchServiceOperation Download();
        IBatchServiceOperation Upload();
    }

    public interface IBatchServiceOperation:
        IStoragrParams<IEnumerable<StoragrBatchObject>, IBatchParams>
    {
        
    }
}