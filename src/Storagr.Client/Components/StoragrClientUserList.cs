using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientUserList : 
        IStoragrClientUserList, 
        IStoragrUserParams
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly StoragrUserListArgs _listArgs;
        
        public StoragrClientUserList(IStoragrClientRequest clientRequest)
        {
            _clientRequest = clientRequest;
            _listArgs = new StoragrUserListArgs();
        }
        
        IStoragrList<StoragrUser> IStoragrRunner<IStoragrList<StoragrUser>>.Run() => (this as IStoragrRunner<IStoragrList<StoragrUser>>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        async Task<IStoragrList<StoragrUser>> IStoragrRunner<IStoragrList<StoragrUser>>.RunAsync(CancellationToken cancellationToken)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await _clientRequest.Send<StoragrUserList>(
                $"users?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        public IStoragrListable<StoragrUser, IStoragrUserParams> Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        public IStoragrListable<StoragrUser, IStoragrUserParams> Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        public IStoragrListable<StoragrUser, IStoragrUserParams> SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        public IStoragrListable<StoragrUser, IStoragrUserParams> Where(Action<IStoragrUserParams> whereParams)
        {
            whereParams(this);
            return this;
        }


        public IStoragrUserParams Id(string userId)
        {
            _listArgs.Id = userId;
            return this;
        }

        public IStoragrUserParams Username(string username)
        {
            _listArgs.Username = username;
            return this;
        }

        public IStoragrUserParams Password(string password)
        {
            // skipping - you cannot filter for password
            return this;
        }

        public IStoragrUserParams IsEnabled(bool isEnabled)
        {
            _listArgs.IsEnabled = isEnabled;
            return this;
        }

        public IStoragrUserParams IsAdmin(bool isAdmin)
        {
            _listArgs.IsAdmin = isAdmin;
            return this;
        }
    }
}