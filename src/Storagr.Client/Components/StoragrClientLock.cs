using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Storagr.Shared.Data;

namespace Storagr.Client
{
    internal class StoragrClientLock :
        StoragrClientHelper,
        IStoragrClientLock
    {
        private readonly string _repositoryIdOrName;
        private readonly string _lockIdOrPath;

        private bool _deleteRequest;
        private StoragrUpdateRequest _createRequest;

        public StoragrClientLock(IStoragrClientRequest clientRequest, string repositoryIdOrName, string lockIdOrPath) 
            : base(clientRequest)
        {
            _repositoryIdOrName = repositoryIdOrName;
            _lockIdOrPath = lockIdOrPath;
        }

        StoragrLock IStoragrClientRunner<StoragrLock>.Run()
        {
            var task = (this as IStoragrClientLock).RunAsync();
            task.RunSynchronously();
            return task.Result;
        }

        Task<StoragrLock> IStoragrClientRunner<StoragrLock>.RunAsync(CancellationToken cancellationToken)
        {
            if (_createRequest is not null)
                return Request<StoragrLock, StoragrUpdateRequest>($"repositories/{_repositoryIdOrName}/locks", HttpMethod.Post, _createRequest,
                    cancellationToken);

            return Request<StoragrLock>(
                $"repositories/{_repositoryIdOrName}/locks/{_lockIdOrPath}",
                _deleteRequest
                    ? HttpMethod.Delete
                    : HttpMethod.Get,
                cancellationToken);
        }

        IStoragrClientRunner<StoragrLock> IStoragrClientCreatable<StoragrLock>.Create()
        {
            _createRequest = new StoragrUpdateRequest();
            _createRequest.Updates.Add("path", _lockIdOrPath);
            return this;
        }

        IStoragrClientRunner<StoragrLock> IStoragrClientDeletable<StoragrLock>.Delete()
        {
            _deleteRequest = true;
            return this;
        }
    }
}