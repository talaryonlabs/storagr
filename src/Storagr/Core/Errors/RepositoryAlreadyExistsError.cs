using System.Runtime.Serialization;
using Storagr.Data;

namespace Storagr
{
    [DataContract]
    public sealed class RepositoryAlreadyExistsError : ConflictError
    {
        [DataMember(Name = "repository")] public StoragrRepository Repository;
        
        public RepositoryAlreadyExistsError(StoragrRepository existingRepository)
            : base("Repository already exists.")
        {
            Repository = existingRepository;
        }
    }
}