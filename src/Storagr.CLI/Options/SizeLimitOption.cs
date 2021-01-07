using System.CommandLine;
using System.CommandLine.Parsing;
using Storagr.Shared;

namespace Storagr.CLI
{
    public class SizeLimitOption : StoragrOption<ulong>
    {
        public SizeLimitOption()
            : base(new[] {"--size-limit"}, ParseNamedSize, false, StoragrConstants.SizeLimitOptionDescription)
        {
        }
    }
}