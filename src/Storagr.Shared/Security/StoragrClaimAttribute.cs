using System;

namespace Storagr.Shared.Security
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class StoragrClaimAttribute : Attribute
    {
        public string Name { get; set; }
    }
}