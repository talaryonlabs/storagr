using System.Runtime.Serialization;

namespace Storagr
{
    [DataContract]
    public sealed class RepositoryNotFoundError : NotFoundError
    {
        public RepositoryNotFoundError() 
            : base("Repository not found.")
        {
            
        }
    }
}