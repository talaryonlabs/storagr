using System.CommandLine;

namespace Storagr.CLI
{
    public class AsJsonOption : StoragrOption<bool>
    {
        public AsJsonOption()
            : base(new[] {"--as-json"}, StoragrConstants.AsJsonOptionDescription)
        {
        }
    }
}