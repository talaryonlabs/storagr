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
    public class StoragrOptions<T> : IOptions<T> where T : class
    {
        T IOptions<T>.Value => (T)(object)this;
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
    
    public class StoragerMediaTypeHeader : MediaTypeHeaderValue
    {
        public StoragerMediaTypeHeader() : base("application/vnd.git-lfs+json") { }
    }
    
    public class StoragrRepositoryNotFoundException : Exception
    {
        public StoragrRepositoryNotFoundException()
            : base($"Repository not found!")
        {
        }
    }
    
    public class StoragrLockExistsException : Exception
    {
        public StoragrLockExistsException()
            : base($"Lock exists!")
        {
        }
    }
}