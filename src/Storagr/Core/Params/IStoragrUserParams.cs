namespace Storagr
{
    public interface IStoragrUserParams
    {
        IStoragrUserParams Id(string userId);
        
        IStoragrUserParams Username(string username);
        IStoragrUserParams Password(string password);

        IStoragrUserParams IsEnabled(bool isEnabled);
        IStoragrUserParams IsAdmin(bool isAdmin);
    }
}