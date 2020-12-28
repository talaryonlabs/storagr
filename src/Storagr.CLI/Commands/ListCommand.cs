using System.CommandLine;

namespace Storagr.CLI
{
    public class ListCommand : Command
    {
        public ListCommand() 
            : base("list", "List resources")
        {
            base.AddAlias("ls");

            AddCommand(new Command("repositories", "Get all repositories")
            {
                new LimitOption(),
                new CursorOption()
            });
            
            AddCommand(new Command("users", "Get all users")
            {
                new LimitOption(),
                new CursorOption()
            });
            
            AddCommand(new Command("objects", "Get all objects from a repository")
            {
                new RepositoryOption(),
                new LimitOption(),
                new CursorOption(),
            });
            
            AddCommand(new Command("locks", "Get all locks from a repository")
            {
                new RepositoryOption(),
                new LimitOption(),
                new CursorOption(),
            });
        }
    }
}