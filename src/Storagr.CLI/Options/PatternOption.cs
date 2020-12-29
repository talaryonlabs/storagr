using System.CommandLine;

namespace Storagr.CLI
{
    public class IdPatternOption : Option<string>
    {
        public IdPatternOption()
            : base(new[] {"--id-pattern"}, StoragrConstants.IdPatternOptionDescription)
        {

        }
    }

    public class PathPatternOption : Option<string>
    {
        public PathPatternOption()
            : base(new[] {"--path-pattern"}, StoragrConstants.PathPatternOptionDescription)
        {

        }
    }
}