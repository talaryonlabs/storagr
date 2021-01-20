using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace Storagr.Shared.Data
{
    [JsonObject]
    public class StoragrRequest<T>
    {
        [JsonProperty("items")]
        public IDictionary<string, object> Items { get; set; } = new Dictionary<string, object>();

        public static T operator +(T a, StoragrRequest<T> b)
        {
            return b + a;
        }
        
        public static T operator +(StoragrRequest<T> a, T b)
        {
            var obj = Activator.CreateInstance<T>();
            foreach (var property in typeof(T).GetProperties())
            {
                var attribute = property.GetCustomAttribute<JsonPropertyAttribute>();
                if (attribute is not null && a.Items.ContainsKey(attribute.PropertyName ?? property.Name))
                {
                    property.SetValue(obj, a.Items[attribute.PropertyName ?? property.Name]);
                }
                else
                {
                    property.SetValue(obj, property.GetValue(b));
                }
            }

            return obj;
        }
    }
}