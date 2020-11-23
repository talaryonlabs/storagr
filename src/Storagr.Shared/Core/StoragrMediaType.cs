using Microsoft.Net.Http.Headers;

namespace Storagr.Shared
{
    public class StoragerMediaType : MediaTypeHeaderValue
    {
        public StoragerMediaType() : base("application/vnd.git-lfs+json") { }
    }
}