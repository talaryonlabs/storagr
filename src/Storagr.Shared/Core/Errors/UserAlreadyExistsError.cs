using System.Runtime.Serialization;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public sealed class UserAlreadyExistsError : ConflictError
    {
        [DataMember(Name = "user")] public StoragrUser User;
        
        public UserAlreadyExistsError(StoragrUser existingUser) 
            : base("User already exists.")
        {
            User = existingUser;
        }
    }
}