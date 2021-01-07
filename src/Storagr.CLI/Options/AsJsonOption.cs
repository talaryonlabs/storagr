using System.CommandLine;

namespace Storagr.CLI
{
    public class AsJsonOption : Option<bool>
    {
        public AsJsonOption()
            : base(new[] {"--as-json"}, StoragrConstants.AsJsonOptionDescription)
        {
        }
    }
}