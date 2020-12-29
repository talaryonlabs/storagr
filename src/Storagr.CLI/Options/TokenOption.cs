using System.CommandLine;

namespace Storagr.CLI
{
    public class TokenOption : Option<string>
    {
        public TokenOption()
            : base(new []{"--token"}, StoragrConstants.TokenOptionDescription)
        {
        }
    }
}