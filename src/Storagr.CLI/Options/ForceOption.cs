using System.CommandLine;

namespace Storagr.CLI
{
    public class ForceOption : Option
    {
        public ForceOption()
            : base(new[] {"-f", "--force"}, StoragrConstants.ForceOptionDescription)
        {

        }
    }
}