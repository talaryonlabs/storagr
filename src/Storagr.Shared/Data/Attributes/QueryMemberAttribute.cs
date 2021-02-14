using System;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared.Data
{
    [AttributeUsage(AttributeTargets.Property)]
    public class QueryMemberAttribute : FromQueryAttribute
    {
        public QueryMemberAttribute(string memberName)
        {
            Name = memberName;
        }
    }
}