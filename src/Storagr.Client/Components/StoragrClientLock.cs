using System;
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

        private bool _deleteRequest;
        private StoragrUpdateRequest _createRequest;

        public StoragrClientLock(IStoragrClientRequest clientRequest, string repositoryIdOrName, string lockIdOrPath) 
            : base(clientRequest)
        {
            _repositoryIdOrName = repositoryIdOrName;
            _lockIdOrPath = lockIdOrPath;
        }

        protected override Task<StoragrLock> RunAsync(IStoragrClientRequest clientRequest, CancellationToken cancellationToken = default)
        {
            if (_createRequest is not null)
                return clientRequest.Send<StoragrLock, StoragrUpdateRequest>($"repositories/{_repositoryIdOrName}/locks", HttpMethod.Post, _createRequest,
                    cancellationToken);

            return clientRequest.Send<StoragrLock>(
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

        IStoragrClientRunner<StoragrLock> IStoragrClientDeletable<StoragrLock>.Delete(bool force)
        {
            _deleteRequest = true;
            return this;
        }
    }
}