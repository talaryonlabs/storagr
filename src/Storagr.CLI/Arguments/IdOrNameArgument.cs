using System.CommandLine;

namespace Storagr.CLI
{
    public class IdOrNameArgument : Argument<string>
    {
        public IdOrNameArgument()
            : base("idOrName", StoragrConstants.IdOrNameArgumentDescription)
        {

        }
    }
}