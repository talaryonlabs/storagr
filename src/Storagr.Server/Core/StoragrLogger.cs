using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Storagr.Server.Data.Entities;

namespace Storagr.Server
{
    public interface IStoragrLoggerProvider : ILoggerProvider
    {
        void Enable();
    }
    
    public static class StoragrLoggerExtension
    {
        public static ILoggingBuilder AddStoragr(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<IStoragrLoggerProvider, StoragrLoggerProvider>();
            
            return builder;
        }
    }
    
    public class StoragrLogger : ILogger
    {
        private readonly string _name;
        private readonly bool _enabled;
        private readonly IDatabaseAdapter _database;

        public StoragrLogger(string name, bool enabled, IDatabaseAdapter database)
        {
            _name = name;
            _enabled = enabled;
            _database = database;
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
            _database?.Insert(log);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _enabled; // TODO
        }

        public IDisposable BeginScope<TState>(TState state) => default;
    }

    public class StoragrLoggerProvider : IStoragrLoggerProvider
    {
        private readonly IDatabaseAdapter _databaseAdapter;
        private readonly ConcurrentDictionary<string, StoragrLogger> _loggers = new();
        
        private bool _enabled;

        public StoragrLoggerProvider(IDatabaseAdapter databaseAdapter)
        {
            _databaseAdapter = databaseAdapter;
        }

        public void Enable()
        {
            _enabled = true;
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new StoragrLogger(name, _enabled, _databaseAdapter));

        public void Dispose() =>
            _loggers.Clear();
    }
}