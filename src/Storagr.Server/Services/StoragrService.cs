using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Storagr.Server.Services
{
    public partial class StoragrService : 
        IStoragrService 
    {
        private IDatabaseAdapter Database { get; }
        private IAuthenticationAdapter Authenticator { get; }
        private IStoreAdapter Store { get; }
        private ICacheService Cache { get; }
        public ITokenService TokenService { get; }
        private DistributedCacheEntryOptions CacheEntryOptions { get; }
        
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StoragrService(IDatabaseAdapter database, IAuthenticationAdapter authenticator, IStoreAdapter store,
            ICacheService cache, ITokenService tokenService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            Authenticator = authenticator;
            Store = store;
            Database = database;
            Cache = cache;
            TokenService = tokenService;
            CacheEntryOptions = new DistributedCacheEntryOptions();
        }

        public IStoragrServiceRepository Repository(string repositoryIdOrName) =>
            new RepositoryItem(this, repositoryIdOrName);

        public IStoragrServiceRepositories Repositories() =>
            new RepositoryList(this);

        public IStoragrServiceUser User(string userIdOrName) => 
            new UserItem(this, userIdOrName);

        public IStoragrServiceUsers Users() =>
            new UserList(this);

        public IStoragrLogList Logs() =>
            new LogList(this);

        public IStoragrServiceAuthorization Authorization() =>
            new Authentication(this, _httpContextAccessor);
    }
}