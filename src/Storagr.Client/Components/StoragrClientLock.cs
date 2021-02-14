using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr;
using Storagr.Shared;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientLock :
        IStoragrClientLock,
        IStoragrRunner<bool>
    {
        private readonly IStoragrClientRequest _clientRequest;
        private readonly string _repositoryIdOrName;
        private readonly string _lockIdOrPath;

        private StoragrLockRequest _lockRequest;
        private StoragrUnlockRequest _unlockRequest;
        
        public StoragrClientLock(IStoragrClientRequest clientRequest, string repositoryIdOrName, string lockIdOrPath) 
        {
            _clientRequest = clientRequest;
            _repositoryIdOrName = repositoryIdOrName;
            _lockIdOrPath = lockIdOrPath;
        }
        
        StoragrLock IStoragrRunner<StoragrLock>.Run() => (this as IStoragrRunner<StoragrLock>)
            .RunAsync()
            .RunSynchronouslyWithResult();

        Task<StoragrLock> IStoragrRunner<StoragrLock>.RunAsync(CancellationToken cancellationToken)
        {
            if (_lockRequest is not null)
                return _clientRequest.Send<StoragrLock, StoragrLockRequest>(
                    $"repositories/{_repositoryIdOrName}/locks",
                    HttpMethod.Post,
                    _lockRequest,
                    cancellationToken
                );
            
            if (_unlockRequest is not null)
                return _clientRequest.Send<StoragrLock, StoragrUnlockRequest>(
                    $"repositories/{_repositoryIdOrName}/locks/{_lockIdOrPath}/unlock",
                    HttpMethod.Post,
                    _unlockRequest,
                    cancellationToken
                );

            return _clientRequest.Send<StoragrLock>(
                $"repositories/{_repositoryIdOrName}/locks/{_lockIdOrPath}",
                HttpMethod.Get,
                cancellationToken
            );
        }

        IStoragrRunner<StoragrLock> IStoragrCreatable<StoragrLock>.Create()
        {
            _lockRequest = new StoragrLockRequest()
            {
                Path = _lockIdOrPath
            };
            return this;
        }

        IStoragrRunner<StoragrLock> IStoragrDeletable<StoragrLock>.Delete(bool force)
        {
            _unlockRequest = new StoragrUnlockRequest()
            {
                Force = force
            };
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
                await (this as IStoragrRunner<StoragrLock>).RunAsync(cancellationToken);
            }
            catch (UserNotFoundError)
            {
                return false;
            }

            return true;
        }
    }
}