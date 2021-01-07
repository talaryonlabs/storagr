using System;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Storagr.Data.Entities;

namespace Storagr
{
    public static class StoragrLoggerExtension
    {
        public static ILoggingBuilder AddStoragr(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerProvider, StoragrLoggerProvider>();
            
            return builder;
        }
    }
    
    public class StoragrLogger : ILogger
    {
        private readonly string _name;
        private readonly IDatabaseAdapter _backendAdapter;

        public StoragrLogger(string name, IDatabaseAdapter backendAdapter)
        {
            _name = name;
            _backendAdapter = backendAdapter;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;
            
            var log = new LogEntity()
            {
                Level = logLevel,
                Date = DateTime.Now,
                Category = _name,
                Message = exception is not null ? exception.Message : formatter(state, exception),
                Exception = exception is not null ? exception.StackTrace : ""
            };
            _backendAdapter?.Insert(log);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true; // TODO
        }

        public IDisposable BeginScope<TState>(TState state) => default;
    }

    public class StoragrLoggerProvider : ILoggerProvider
    {
        private readonly IDatabaseAdapter _backendAdapter;
        private readonly ConcurrentDictionary<string, StoragrLogger> _loggers =
            new ConcurrentDictionary<string, StoragrLogger>();

        public StoragrLoggerProvider(IDatabaseAdapter backendAdapter)
        {
            _backendAdapter = backendAdapter;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new StoragrLogger(name, _backendAdapter));

        public void Dispose() =>
            _loggers.Clear();
    }
}