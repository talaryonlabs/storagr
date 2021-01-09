using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    public interface IUserCreator : IStoragrCreator<StoragrUser>
    {
        IUserCreator SetPassword(string password);
        IUserCreator SetEnabled(bool isEnabled);
        IUserCreator SetAdmin(bool isAdmin);
    }
    
    public interface IUserUpdater : IStoragrUpdater<StoragrUser>
    {
        IUserUpdater SetUsername(string newUsername);
        IUserUpdater SetPassword(string newPassword);
        
        IUserUpdater SetEnabled(bool isEnabled);
        IUserUpdater SetAdmin(bool isAdmin);
    }
    
    public partial class StoragrClient
    {
        private class UserCreator : StoragrClientHelper, IUserCreator
        {
            private readonly StoragrUserRequest _userRequest;
            
            public UserCreator(string username, StoragrClient storagrClient, HttpClient httpClient)
                : base(storagrClient, httpClient)
            {
                _userRequest = new StoragrUserRequest()
                {
                    User = new StoragrUser()
                    {
                        Username = username
                    }
                };
            }

            public IUserCreator SetPassword(string newPassword)
            {
                _userRequest.NewPassword = newPassword;
                return this;
            }

            public IUserCreator SetEnabled(bool isEnabled)
            {
                _userRequest.User.IsEnabled = isEnabled;
                return this;
            }

            public IUserCreator SetAdmin(bool isAdmin)
            {
                _userRequest.User.IsAdmin = isAdmin;
                return this;
            }

            public async Task<StoragrUser> Create(CancellationToken cancellationToken = default)
            {
                var request = StoragrClient.CreateRequest($"users", HttpMethod.Post, _userRequest);
                var response = await HttpClient.SendAsync(request, cancellationToken);
                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                
                if (!response.IsSuccessStatusCode) 
                    throw (StoragrError) data;

                return data;
            }
        }
        
        private class UserUpdater : StoragrClientHelper, IUserUpdater
        {
            private readonly string _userId;
            private readonly StoragrUpdateRequest _userUpdateRequest;
            
            public UserUpdater(string userId, StoragrClient storagrClient, HttpClient httpClient)
            : base(storagrClient, httpClient)
            {
                _userId = userId;
                _userUpdateRequest = new StoragrUpdateRequest()
                {
                    Type = StoragrUpdateType.User
                };
            }

            public IUserUpdater SetUsername(string newUsername)
            {
                _userUpdateRequest.Updates.Add("username", newUsername);
                return this;
            }

            public IUserUpdater SetPassword(string newPassword)
            {
                _userUpdateRequest.Updates.Add("password", newPassword);
                return this;
            }

            public IUserUpdater SetEnabled(bool isEnabled)
            {
                _userUpdateRequest.Updates.Add("is_enabled", isEnabled);
                return this;
            }

            public IUserUpdater SetAdmin(bool isAdmin)
            {
                _userUpdateRequest.Updates.Add("is_admin", isAdmin);
                return this;
            }

            public async Task<StoragrUser> Update(CancellationToken cancellationToken = default)
            {
                var request = StoragrClient.CreateRequest($"repositories/{_userId}", HttpMethod.Patch, _userUpdateRequest);
                var response = await HttpClient.SendAsync(request, cancellationToken);
                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                
                if (!response.IsSuccessStatusCode) 
                    throw (StoragrError) data;

                return data;
            }
        }
    }
}