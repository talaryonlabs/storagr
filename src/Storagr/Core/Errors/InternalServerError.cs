using System;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Http;

namespace Storagr
{
    [DataContract]
    public sealed class InternalServerError : StoragrError
    {
        public InternalServerError()
            : base(StatusCodes.Status500InternalServerError, "Unknown Error")
        {
        }

        public InternalServerError(string message)
            : base(StatusCodes.Status500InternalServerError, message)
        {
        }

        public InternalServerError(Exception exception)
            : base(StatusCodes.Status500InternalServerError, exception)
        {
        }
    }
}