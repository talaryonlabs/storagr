using System.CommandLine;

namespace Storagr.CLI
{
    public class HostOption : Option<string>
    {
        public HostOption()
            : base(new []{"--host"}, StoragrConstants.HostOptionDescription)
        {
        }
    }
}