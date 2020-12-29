using System.CommandLine;

namespace Storagr.CLI
{
    public class TimelineCommand : Command
    {
        public TimelineCommand() 
            : base("timeline", StoragrConstants.TimelineCommandDescription)
        {
            AddOption(new RepositoryOption());
        }
    }
}