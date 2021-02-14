using System;

namespace Storagr.Shared.Security
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class TokenClaimAttribute : Attribute
    {
        public string Name { get; set; }
    }
}