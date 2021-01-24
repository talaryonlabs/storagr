namespace Storagr
{
    public interface IStoragrLogParams
    {
        IStoragrLogParams Message(string logId);
        IStoragrLogParams Level(Microsoft.Extensions.Logging.LogLevel logLevel);
        IStoragrLogParams Category(string category);
    }
}