using System.CommandLine;

namespace Storagr.CLI
{
    public class GetCommand : Command
    {
        public GetCommand()
            : base("get", "")
        {
            AddCommand(new Command("user")
            {
                
            });
            AddCommand(new Command("repository")
            {
                
            });
            AddCommand(new Command("object")
            {
                new RepositoryOption()
            });
            AddCommand(new Command("lock")
            {
                new RepositoryOption()
            });
        }
    }
}