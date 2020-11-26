using System;

namespace Storagr.Shared.Security
{
    public class StoragrTokenMemberAttribute : Attribute
    {
        public string Name { get; set; }
        public string ClaimType { get; set; }
    }
}