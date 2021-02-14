using System;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Storagr.Shared
{
    public class StoragrFileLogger : ILogger
    {
        private readonly IFileInfo _logFile;

        public StoragrFileLogger(IFileInfo logFile)
        {
            _logFile = logFile;
        }
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public IDisposable BeginScope<TState>(TState state) => default;
    }
}