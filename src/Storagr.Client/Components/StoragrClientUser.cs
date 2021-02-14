using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientUser : 
        IStoragrParams<StoragrUser, IStoragrUserParams>, 
        IStoragrClientUser, 
        IStoragrUserParams,
        IStoragrRunner<bool>
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly string _userIdOrName;

        private bool _userDeleting;
        
        private StoragrRequest<StoragrUser> _createRequest;
        private StoragrRequest<StoragrUser> _updateRequest;
        
        public StoragrClientUser(IStoragrClientRequest clientRequest, string userIdOrName)
        {
            _clientRequest = clientRequest;
            _userIdOrName = userIdOrName;
        }
        
        StoragrUser IStoragrRunner<StoragrUser>.Run() => (this as IStoragrRunner<StoragrUser>)
            .RunAsync()
            .RunSynchronouslyWithResult();
        
        Task<StoragrUser> IStoragrRunner<StoragrUser>.RunAsync(CancellationToken cancellationToken)
        {
            if (_createRequest is not null)
                return _clientRequest.Send<StoragrUser, StoragrRequest<StoragrUser>>(
                    $"users",
                    HttpMethod.Post,
                    _createRequest,
                    cancellationToken
                );

            if (_updateRequest is not null)
                return _clientRequest.Send<StoragrUser, StoragrRequest<StoragrUser>>(
                    $"users/{_userIdOrName}",
                    HttpMethod.Patch,
                    _updateRequest,
                    cancellationToken
                );

            return _clientRequest.Send<StoragrUser>(
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


        public IStoragrRunner<bool> Exists() => this;

        bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                await (this as IStoragrRunner<StoragrUser>).RunAsync(cancellationToken);
            }
            catch (UserNotFoundError)
            {
                return false;
            }

            return true;
        }
    }
}