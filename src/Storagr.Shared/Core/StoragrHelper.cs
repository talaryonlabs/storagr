using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Newtonsoft.Json;

namespace Storagr.Shared
{
    public static class StoragrHelper
    {
        [Pure]
        public static byte[] SerializeObject<T>(T obj)
        {
            var p = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(p);
        }
        [Pure]
        public static T DeserializeObject<T>(byte[] data)
        {
            var p = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(p);
        }

        // ReSharper disable once InconsistentNaming
        [Pure]
        public static string UUID()
        {
            return Guid.NewGuid().ToString();
        }

        [Pure]
        public static long ParseNamedSize(string namedSize)
        {
            var names = new[] {"K", "M", "G", "T", "P", "E", "Z", "Y"};
            var alias = new[] {"KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

            for (var i = 0; i < names.Length; i++)
                if (namedSize.EndsWith(names[i], true, null))
                {
                    return long.Parse(namedSize.Substring(0, namedSize.Length - 1)) * (long)Math.Pow(1024, i + 1);
                }
                else if (namedSize.EndsWith(alias[i], true, null))
                {
                    return long.Parse(namedSize.Substring(0, namedSize.Length - 2)) * (long)Math.Pow(1024, i + 1);
                }

            return long.Parse(namedSize);
        }
        
        [Pure]
        public static TimeSpan ParseNamedDelay(string namedDelay)
        {
            var delays = new[] {"d", "h", "m", "s", "ms"};

            foreach (var delay in delays)
                if (namedDelay.EndsWith(delay, true, null))
                {
                    var value = namedDelay.Substring(0, namedDelay.Length - delay.Length);
                    var number = double.Parse(value);

                    return delay switch
                    {
                        "d" => TimeSpan.FromDays(number),
                        "h" => TimeSpan.FromHours(number),
                        "m" => TimeSpan.FromMinutes(number),
                        "s" => TimeSpan.FromSeconds(number),
                        "ms" => TimeSpan.FromMilliseconds(number),
                        _ => throw new NotImplementedException()
                    };
                }

            return TimeSpan.FromSeconds(double.Parse(namedDelay));
        }
    }
}