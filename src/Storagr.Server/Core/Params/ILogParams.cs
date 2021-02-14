using Microsoft.Extensions.Logging;

namespace Storagr.Server
{
    public interface ILogParams
    {
        ILogParams Level(LogLevel level);
        ILogParams Category(string catgeory);
        ILogParams Message(string message);
    }
}