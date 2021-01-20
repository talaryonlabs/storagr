using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientUserList : 
        StoragrClientHelper<IStoragrList<StoragrUser>>, 
        IStoragrClientUserList, 
        IStoragrUserParams
    {
        private readonly StoragrUserListArgs _listArgs;
        
        public StoragrClientUserList(IStoragrClientRequest clientRequest) 
            : base(clientRequest)
        {
            _listArgs = new StoragrUserListArgs();
        }

        protected override async Task<IStoragrList<StoragrUser>> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            var query = StoragrHelper.ToQueryString(_listArgs);
            return await clientRequest.Send<StoragrUserList>(
                $"users?{query}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        public IStoragrClientList<StoragrUser, IStoragrUserParams> Take(int count)
        {
            _listArgs.Limit = count;
            return this;
        }

        public IStoragrClientList<StoragrUser, IStoragrUserParams> Skip(int count)
        {
            _listArgs.Skip = count;
            return this;
        }

        public IStoragrClientList<StoragrUser, IStoragrUserParams> SkipUntil(string cursor)
        {
            _listArgs.Cursor = cursor;
            return this;
        }

        public IStoragrClientList<StoragrUser, IStoragrUserParams> Where(Action<IStoragrUserParams> whereParams)
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