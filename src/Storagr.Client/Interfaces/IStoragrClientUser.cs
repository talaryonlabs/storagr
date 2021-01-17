using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IStoragrUserParams
    {
        IStoragrUserParams Id(string userId);
        
        IStoragrUserParams Username(string username);
        IStoragrUserParams Password(string password);

        IStoragrUserParams IsEnabled(bool isEnabled);
        IStoragrUserParams IsAdmin(bool isAdmin);
    }

    public interface IStoragrClientUser :
        IStoragrClientRunner<StoragrUser>,
        IStoragrClientCreatable<StoragrUser, IStoragrUserParams>,
        IStoragrClientUpdatable<StoragrUser, IStoragrUserParams>,
        IStoragrClientDeletable<StoragrUser>
    {

    }

    public interface IStoragrClientUserList :
        IStoragrClientList<StoragrUser, IStoragrUserParams>
    {

    }
}