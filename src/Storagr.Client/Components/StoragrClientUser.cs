using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Data;

namespace Storagr.Client
{
    internal class StoragrClientUser : 
        StoragrClientHelper<StoragrUser>, 
        IStoragrParams<StoragrUser, IStoragrUserParams>, 
        IStoragrUser, 
        IStoragrUserParams
    {
        private readonly string _userIdOrName;

        private bool _userDeleting;
        
        private StoragrRequest<StoragrUser> _createRequest;
        private StoragrRequest<StoragrUser> _updateRequest;
        
        public StoragrClientUser(IStoragrClientRequest clientRequest, string userIdOrName) : 
            base(clientRequest)
        {
            _userIdOrName = userIdOrName;
            
        }

        protected override Task<StoragrUser> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            if (_createRequest is not null)
                return clientRequest.Send<StoragrUser, StoragrRequest<StoragrUser>>(
                    $"users",
                    HttpMethod.Post,
                    _createRequest,
                    cancellationToken
                );

            if (_updateRequest is not null)
                return clientRequest.Send<StoragrUser, StoragrRequest<StoragrUser>>(
                    $"users/{_userIdOrName}",
                    HttpMethod.Patch,
                    _updateRequest,
                    cancellationToken
                );

            return clientRequest.Send<StoragrUser>(
                $"users/{_userIdOrName}",
                _userDeleting
                    ? HttpMethod.Delete
                    : HttpMethod.Get,
                cancellationToken);
        }

        public IStoragrRunner<StoragrUser> With(Action<IStoragrUserParams> withParams)
        {
            withParams(this);
            return this;
        }

        IStoragrParams<StoragrUser, IStoragrUserParams> IStoragrCreatable<StoragrUser, IStoragrUserParams>.Create()
        {
            _createRequest = new StoragrRequest<StoragrUser>();
            return this;
        }

        IStoragrParams<StoragrUser, IStoragrUserParams> IStoragrUpdatable<StoragrUser, IStoragrUserParams>.Update()
        {
            _updateRequest = new StoragrRequest<StoragrUser>();
            return this;
        }

        IStoragrRunner<StoragrUser> IStoragrDeletable<StoragrUser>.Delete(bool force)
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
            (_createRequest ?? _updateRequest).Items.Add("username", username);
            return this;
        }

        IStoragrUserParams IStoragrUserParams.Password(string password)
        {
            (_createRequest ?? _updateRequest).Items.Add("password", password);
            return this;
        }

        IStoragrUserParams IStoragrUserParams.IsEnabled(bool isEnabled)
        {
            (_createRequest ?? _updateRequest).Items.Add("is_enabled", isEnabled);
            return this;
        }

        IStoragrUserParams IStoragrUserParams.IsAdmin(bool isAdmin)
        {
            (_createRequest ?? _updateRequest).Items.Add("is_admin", isAdmin);
            return this;
        }
    }
}