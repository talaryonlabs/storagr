using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Storagr.CLI
{
    public interface IConfigService
    {
        bool Exists(string name);
        void Set(string name, string value);
        string Get(string name);
        IReadOnlyDictionary<string, string> GetAll();
    }

    public class ConfigService : IConfigService
    {
        private readonly string _path;
        private readonly Dictionary<string, string> _data;
        
        public ConfigService()
        {
            _path = "/usr/storagr/config/storagr.cli.json";
            try
            {
                using var reader = File.OpenText(_path);
                _data = (Dictionary<string, string>)new JsonSerializer().Deserialize(reader, typeof(Dictionary<string, string>));
            }
            catch (Exception)
            {
                _data = new Dictionary<string, string>();
            }
        }

        public bool Exists(string name) => _data.ContainsKey(name);

        public void Set(string name, string value)
        {
            if (!_data.ContainsKey(name))
                _data.Add(name, value);
            else
                _data[name] = value;

            using var writer = File.CreateText(_path);
            new JsonSerializer().Serialize(writer, _data);
        }
        
        public string Get(string name) => _data[name];
        
        public IReadOnlyDictionary<string, string> GetAll() => _data;
    }
}