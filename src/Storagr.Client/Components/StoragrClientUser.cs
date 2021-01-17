using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientUser : 
        StoragrClientHelper<StoragrUser>, 
        IStoragrClientParams<StoragrUser, IStoragrUserParams>, 
        IStoragrClientUser, 
        IStoragrUserParams
    {
        private readonly string _userIdOrName;

        private bool _userDeleting;
        
        private StoragrUpdateRequest _userCreateRequest;
        private StoragrUpdateRequest _userUpdateRequest;
        
        public StoragrClientUser(IStoragrClientRequest clientRequest, string userIdOrName) : 
            base(clientRequest)
        {
            _userIdOrName = userIdOrName;
            
        }

        protected override Task<StoragrUser> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            if (_userCreateRequest is not null)
                return clientRequest.Send<StoragrUser, StoragrUpdateRequest>(
                    $"users",
                    HttpMethod.Post,
                    _userCreateRequest,
                    cancellationToken
                );

            if (_userUpdateRequest is not null)
                return clientRequest.Send<StoragrUser, StoragrUpdateRequest>(
                    $"users/{_userIdOrName}",
                    HttpMethod.Patch,
                    _userUpdateRequest,
                    cancellationToken
                );

            return clientRequest.Send<StoragrUser>(
                $"users/{_userIdOrName}",
                _userDeleting
                    ? HttpMethod.Delete
                    : HttpMethod.Get,
                cancellationToken);
        }

        public IStoragrClientRunner<StoragrUser> With(Action<IStoragrUserParams> withParams)
        {
            withParams(this);
            return this;
        }

        IStoragrClientParams<StoragrUser, IStoragrUserParams> IStoragrClientCreatable<StoragrUser, IStoragrUserParams>.Create()
        {
            _userCreateRequest = new StoragrUpdateRequest() {Type = StoragrUpdateType.User};
            return this;
        }

        IStoragrClientParams<StoragrUser, IStoragrUserParams> IStoragrClientUpdatable<StoragrUser, IStoragrUserParams>.Update()
        {
            _userUpdateRequest = new StoragrUpdateRequest() {Type = StoragrUpdateType.User};
            return this;
        }

        IStoragrClientRunner<StoragrUser> IStoragrClientDeletable<StoragrUser>.Delete(bool force)
        {
            _userDeleting = true;
            return this;
        }

        IStoragrUserParams IStoragrUserParams.Id(string userId)
        {
            // skipping - you cannot create or update UserId
            return this;
        }

        IStoragrUserParams IStoragrUserParams.Username(string username)
        {
            (_userCreateRequest ?? _userUpdateRequest).Updates.Add("username", username);
            return this;
        }

        IStoragrUserParams IStoragrUserParams.Password(string password)
        {
            (_userCreateRequest ?? _userUpdateRequest).Updates.Add("password", password);
            return this;
        }

        IStoragrUserParams IStoragrUserParams.IsEnabled(bool isEnabled)
        {
            (_userCreateRequest ?? _userUpdateRequest).Updates.Add("is_enabled", isEnabled);
            return this;
        }

        IStoragrUserParams IStoragrUserParams.IsAdmin(bool isAdmin)
        {
            (_userCreateRequest ?? _userUpdateRequest).Updates.Add("is_admin", isAdmin);
            return this;
        }
    }
}