using Microsoft.Extensions.Logging;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrLogParams
    {
        IStoragrLogParams Message(string logId);
        IStoragrLogParams Level(LogLevel logLevel);
        IStoragrLogParams Category(string category);
    }
    
    public interface IStoragrLogList : IStoragrClientList<StoragrLog, IStoragrLogParams>
    {
        
    }
}