using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr
{
    [DataContract]
    public class NotImplementedError : StoragrError
    {
        public NotImplementedError() 
            : base(StatusCodes.Status501NotImplemented, "Method not implemented.")
        {
        }
    }
}