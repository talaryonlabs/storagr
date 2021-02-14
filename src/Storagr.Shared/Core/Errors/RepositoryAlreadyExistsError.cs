using System.Runtime.Serialization;
using Storagr.Shared.Data;

namespace Storagr.Shared
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