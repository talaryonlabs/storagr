using System;

namespace Storagr
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheKeyAttribute : Attribute
    {
        public string Name { get; }

        public CacheKeyAttribute(string keyName)
        {
            Name = keyName;
        }
    }
}