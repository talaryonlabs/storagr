using Microsoft.Net.Http.Headers;

namespace Storagr.Shared
{
    public class StoragrMediaType : MediaTypeHeaderValue
    {
        public StoragrMediaType() : base("application/vnd.git-lfs+json") { }
    }
    
    
}