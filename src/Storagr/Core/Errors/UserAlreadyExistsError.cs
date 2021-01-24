using System.Runtime.Serialization;
using Storagr.Data;

namespace Storagr
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