using System.CommandLine;
using System.CommandLine.Invocation;

namespace Storagr.CLI
{
    public class NewCommand : Command
    {
        public NewCommand()
            : base("new", "Creates a resource")
        {
            AddCommand(new Command("user", "Creates a new user")
            {
                new Option("--username"),
            });
            AddCommand(new Command("repository", "Creates a new repository")
            {
                new Argument("name")
                {
                    Description = "Name of the new repository"
                }
            });
        }
    }
}