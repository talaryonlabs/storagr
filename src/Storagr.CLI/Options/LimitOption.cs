using System.CommandLine;

namespace Storagr.CLI
{
    public class LimitOption : Option<int>
    {
        public LimitOption()
            : base(new[] {"--limit"}, StoragrConstants.LimitOptionDescription)
        {
        }
    }
}