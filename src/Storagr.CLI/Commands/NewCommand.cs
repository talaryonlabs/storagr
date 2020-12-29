using System.CommandLine;

namespace Storagr.CLI
{
    public class NewCommand : Command
    {
        public NewCommand()
            : base("new", StoragrConstants.NewCommandDescription)
        {
            AddCommand(new Command("user", StoragrConstants.NewUserCommandDescription)
            {
                new Option("--username"),
            });
            AddCommand(new Command("repository", StoragrConstants.NewRepositoryCommandDescription)
            {
                new Argument("name")
                {
                    Description = "Name of the new repository"
                }
            });
        }
    }
}