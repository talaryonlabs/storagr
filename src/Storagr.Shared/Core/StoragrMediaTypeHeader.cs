using Microsoft.Net.Http.Headers;

namespace Storagr.Shared
{
    public class StoragerMediaTypeHeader : MediaTypeHeaderValue
    {
        public StoragerMediaTypeHeader() : base("application/vnd.git-lfs+json") { }
    }
}