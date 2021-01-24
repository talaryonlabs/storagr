using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Data
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