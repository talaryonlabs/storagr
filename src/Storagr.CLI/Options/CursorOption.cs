using System.CommandLine;

namespace Storagr.CLI
{
    public class CursorOption : Option<string>
    {
        public CursorOption()
            : base(new[] {"--cursor"}, "")
        {

        }
    }
}