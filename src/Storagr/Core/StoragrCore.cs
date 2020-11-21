using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Storagr.Controllers;
using Storagr.IO;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr
{
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

    public class StoragrFeatureProvider : ControllerFeatureProvider
    {
        private readonly StoragrSettings _settings;

        public StoragrFeatureProvider(StoragrSettings settings)
        {
            _settings = settings;
        }
        
        protected override bool IsController(TypeInfo typeInfo)
        {
            return base.IsController(typeInfo);
        }
    }
}