using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientLock :
        StoragrClientHelper<StoragrLock>,
        IStoragrClientLock
    {
        private readonly string _repositoryIdOrName;
        private readonly string _lockIdOrPath;

        private StoragrLockRequest _lockRequest;
        private StoragrUnlockRequest _unlockRequest;
        
        public StoragrClientLock(IStoragrClientRequest clientRequest, string repositoryIdOrName, string lockIdOrPath) 
            : base(clientRequest)
        {
            _repositoryIdOrName = repositoryIdOrName;
            _lockIdOrPath = lockIdOrPath;
        }

        protected override Task<StoragrLock> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            if (_lockRequest is not null)
                return clientRequest.Send<StoragrLock, StoragrLockRequest>(
                    $"repositories/{_repositoryIdOrName}/locks",
                    HttpMethod.Post,
                    _lockRequest,
                    cancellationToken
                );
            
            if (_unlockRequest is not null)
                return clientRequest.Send<StoragrLock, StoragrUnlockRequest>(
                    $"repositories/{_repositoryIdOrName}/locks/{_lockIdOrPath}/unlock",
                    HttpMethod.Post,
                    _unlockRequest,
                    cancellationToken
                );

            return clientRequest.Send<StoragrLock>(
                $"repositories/{_repositoryIdOrName}/locks/{_lockIdOrPath}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrClientRunner<StoragrLock> IStoragrClientCreatable<StoragrLock>.Create()
        {
            _lockRequest = new StoragrLockRequest()
            {
                Path = _lockIdOrPath
            };
            return this;
        }

        IStoragrClientRunner<StoragrLock> IStoragrClientDeletable<StoragrLock>.Delete(bool force)
        {
            _unlockRequest = new StoragrUnlockRequest()
            {
                Force = force
            };
            return this;
        }
    }
}