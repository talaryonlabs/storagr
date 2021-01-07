using System.CommandLine;

namespace Storagr.CLI
{
    public class IdArgument : Argument<string>
    {
        public IdArgument()
            : base("id", StoragrConstants.IdArgumentDescription)
        {

        }
    }
}