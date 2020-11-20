using System;
using System.Diagnostics.Contracts;
using System.Text;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Storagr.Client.Models;
using Storagr.IO;

namespace Storagr
{
    public class StoragrOptions<T> : IOptions<T> where T : class
    {
        T IOptions<T>.Value => (T)(object)this;
    }

    public static class StoragrConverter
    {
        public static BatchAction ToBatchAction(StoreRequest request) => new BatchAction()
        {
            Href = request.Url,
            Header = request.Header,
            ExpiresAt = request.ExpiresAt,
            ExpiresIn = request.ExpiresIn
        };
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