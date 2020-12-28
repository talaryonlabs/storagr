using System.CommandLine;

namespace Storagr.CLI
{
    public class DeleteCommand : Command
    {
        public DeleteCommand()
            : base("delete", "")
        {
            AddCommand(new Command("user", "")
            {
                
            });
            
            AddCommand(new Command("repository", "")
            {
                
            });
            
            AddCommand(new Command("object", "")
            {
                new RepositoryOption()
            });
        }
    }
}