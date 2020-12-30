using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr.Shared
{
    [DataContract]
    public class RepositoryNotFoundError : StoragrError
    {
        public RepositoryNotFoundError() : base(StatusCodes.Status404NotFound, "Repository not found.")
        {
            
        }
    }
}