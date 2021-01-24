using System;

namespace Storagr.Security
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class TokenClaimAttribute : Attribute
    {
        public string Name { get; set; }
    }
}