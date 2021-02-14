using System.Runtime.Serialization;

namespace Storagr.Shared
{
    [DataContract]
    public sealed class UserNotFoundError : NotFoundError
    {
        public UserNotFoundError() 
            : base("User not found.")
        {
        }
    }
}