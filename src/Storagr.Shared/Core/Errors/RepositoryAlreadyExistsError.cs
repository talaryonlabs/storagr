using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public class RepositoryAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "repository")] public StoragrRepository Repository;
        
        public RepositoryAlreadyExistsError(StoragrRepository existingRepository) : base(StatusCodes.Status409Conflict, "Repository already exists.")
        {
            Repository = existingRepository;
        }
    }
}