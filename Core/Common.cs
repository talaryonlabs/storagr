using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Storagr
{
    public class LfsMediaTypeHeader : MediaTypeHeaderValue
    {
        public LfsMediaTypeHeader() : base("application/vnd.git-lfs+json") { }
    }


    public class StoragrOptions : IOptions<StoragrOptions>
    {
        StoragrOptions IOptions<StoragrOptions>.Value => this;
    }

    public class StoragrCacheOptions : StoragrOptions
    {
        
    }
    
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
    }
}