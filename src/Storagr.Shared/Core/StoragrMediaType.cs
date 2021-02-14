using Microsoft.Net.Http.Headers;

namespace Storagr
{
    public class StoragrMediaType : MediaTypeHeaderValue
    {
        public StoragrMediaType() : base("application/vnd.git-lfs+json") { }
    }
    
    
}