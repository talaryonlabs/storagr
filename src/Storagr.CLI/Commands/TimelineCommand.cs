using System.CommandLine;

namespace Storagr.CLI
{
    public class TimelineCommand : StoragrCommand
    {
        private class LocalOptions
        {
            
        }
        
        public TimelineCommand() 
            : base("timeline", StoragrConstants.TimelineCommandDescription)
        {
            AddOption(new RepositoryOption());
        }
    }
}