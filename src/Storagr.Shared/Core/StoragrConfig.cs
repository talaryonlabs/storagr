using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Storagr.Shared
{
    public class StoragrConfig
    {
        private readonly string _rootName;
        private readonly IConfiguration _configuration;
        private readonly List<object> _cache;

        public StoragrConfig(string rootName, IConfiguration configuration)
        {
            _rootName = rootName;
            _configuration = configuration;
            _cache = new List<object>();
        }

        public T Get<T>() => (T) Get(typeof(T));

        public object Get(Type type)
        {
            if (_cache.Exists(v => v.GetType() == type))
            {
                return _cache.FirstOrDefault(v => v.GetType() == type);
            }
            
            var configAttribute = (StoragrConfigAttribute)type.GetCustomAttribute(typeof(StoragrConfigAttribute));
            if (configAttribute == null)
                throw new InvalidOperationException($"{nameof(StoragrConfigAttribute)} missing.");

            var config = (object)Activator.CreateInstance(type);
            
            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.CanWrite && v.GetCustomAttribute(typeof(StoragrConfigValueAttribute)) != null);
            foreach (var property in properties)
            {
                var attribute = (StoragrConfigValueAttribute)property.GetCustomAttribute(typeof(StoragrConfigValueAttribute));
                var value = ReadValue(configAttribute.Name, attribute?.Name ?? property.Name);

                if (value != null) 
                    property.SetValue(config, ParseValue(value, property.PropertyType, attribute));
            }
            _cache.Add(config);

            return config;
        }

        private string ReadValue(string sectionName, string valueName)
        {
            var keys = new List<string>(new[] {_rootName});
            if (!string.IsNullOrEmpty(sectionName))
            {
                keys.Add(sectionName);
            }
            keys.Add(valueName);

            return _configuration[string.Join("_", keys).ToUpper()] ?? _configuration[string.Join(":", keys)];
        }

        private object ParseValue(string value, Type type, StoragrConfigValueAttribute attribute)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, value);
            }
            if (type.IsPrimitive)
            {
                return Convert.ChangeType(attribute.IsNamedSize
                        ? (object) StoragrHelper.ParseNamedSize(value)
                        : value,
                    type);
            }

            if (type == typeof(TimeSpan))
            {
                return attribute.IsNamedDelay ? StoragrHelper.ParseNamedDelay(value) : TimeSpan.Parse(value);
            }
            if (type == typeof(IPEndPoint))
            {
                return IPEndPoint.Parse(value);
            }
            return type.GetCustomAttribute<StoragrConfigAttribute>() != null ? Get(type) : value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class StoragrConfigAttribute : Attribute
    {
        public string Name { get; set; }

        public StoragrConfigAttribute()
        {
        }
        public StoragrConfigAttribute(string name)
        {
            Name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class StoragrConfigValueAttribute : Attribute
    {
        public string Name { get; set; }
        public bool IsNamedSize { get; set; }
        public bool IsNamedDelay { get; set; }
    }
}