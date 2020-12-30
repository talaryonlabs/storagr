using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;
using Storagr.Shared.Data;

namespace Storagr.Shared
{
    [DataContract]
    public class UserAlreadyExistsError : StoragrError
    {
        [DataMember(Name = "user")] public StoragrUser User;
        
        public UserAlreadyExistsError(StoragrUser existingUser) : base(StatusCodes.Status409Conflict, "User already exists.")
        {
            User = existingUser;
        }
    }
}