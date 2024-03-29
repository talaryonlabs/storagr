﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Storagr.Shared.Data;

namespace Storagr
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
        public static ulong ParseNamedSize(string namedSize)
        {
            var names = new[] {"K", "M", "G", "T", "P", "E"}; // "Z", "Y"
            var alias = new[] {"KB", "MB", "GB", "TB", "PB", "EB"}; // "ZB", "YB"

            for (var i = 0; i < names.Length; i++)
                if (namedSize.EndsWith(names[i], true, null))
                {
                    return (ulong) (long.Parse(namedSize.Substring(0, namedSize.Length - 1)) * Math.Pow(1024, i + 1));
                }
                else if (namedSize.EndsWith(alias[i], true, null))
                {
                    return (ulong) (long.Parse(namedSize.Substring(0, namedSize.Length - 2)) * Math.Pow(1024, i + 1));
                }

            return ulong.Parse(namedSize);
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
                        _ => throw new FormatException()
                    };
                }

            return TimeSpan.FromSeconds(double.Parse(namedDelay));
        }

        [Pure]
        public static (string, string, short) ParseHostname(string hostname)
        {
            var position = 0;
            var protocol = default(string);
            var port = (short)-1;


            if (hostname.Contains("://"))
            {
                position = hostname.IndexOf("://", StringComparison.Ordinal);
                protocol = hostname.Substring(0, position);
                hostname = hostname.Substring(position + 3, hostname.Length - 3 - position);
            }

            if (hostname.Contains(":"))
            {
                position = hostname.IndexOf(":", StringComparison.Ordinal) + 1;
                port = short.Parse(hostname.Substring(position, hostname.Length - position));
                hostname = hostname.Substring(0, position - 1);
            }
            
            return (protocol, hostname, port);
        }

        [Pure]
        public static string ToQueryString<T>(T data)
        {
            return string.Join("&", typeof(T)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.CanRead)
                .Select(v =>
                {
                    var attr = v.GetCustomAttributes<QueryMemberAttribute>().FirstOrDefault();
                    var name = attr is not null ? attr.Name : v.Name;
                    var value = v.GetValue(data) ?? "";

                    return $"{name.ToLower()}={HttpUtility.UrlEncode(value.ToString())}";
                }));
        }
    }
}