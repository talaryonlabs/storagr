namespace Storagr.Server
{
    public interface IUserParams
    {
        IUserParams Id(string userId);
        
        IUserParams Username(string username);
        IUserParams Password(string password);

        IUserParams IsEnabled(bool isEnabled);
        IUserParams IsAdmin(bool isAdmin);
    }
}