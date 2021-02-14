using System;
using Microsoft.AspNetCore.Mvc;

namespace Storagr.Shared
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ApiRouteAttribute : RouteAttribute
    {
        public ApiRouteAttribute(string template)
            : base("v{version:apiVersion}" + (template.StartsWith("/") ? "" : "/") + template)
        {
        }
    }
}