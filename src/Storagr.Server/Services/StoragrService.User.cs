using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Storagr.Server.Data.Entities;
using Storagr.Shared;

namespace Storagr.Server.Services
{
    public partial class StoragrService
    {
        private class UserItem :
            IStoragrServiceUser,
            IUserParams,
            IStoragrRunner<bool>,
            IStoragrParams<UserEntity, IUserParams>
        {
            private readonly StoragrService _storagrService;
            private readonly string _userIdOrName;

            private UserEntity _createRequest;
            private Dictionary<string, object> _updateRequest;
            private bool _deleteRequest;

            public UserItem(StoragrService storagrService, string userIdOrName)
            {
                _storagrService = storagrService;
                _userIdOrName = userIdOrName;
            }

            UserEntity IStoragrRunner<UserEntity>.Run() => (this as IStoragrRunner<UserEntity>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<UserEntity> IStoragrRunner<UserEntity>.RunAsync(CancellationToken cancellationToken)
            {
                var cachedUser = await _storagrService
                    .Cache
                    .Key<UserEntity>(StoragrCaching.GetUserKey(_userIdOrName))
                    .RunAsync(cancellationToken);
                var userEntity = cachedUser ?? await _storagrService
                    .Database
                    .First<UserEntity>()
                    .Where(filter => filter
                        .Is(nameof(UserEntity.Id))
                        .EqualTo(_userIdOrName)
                        .Or()
                        .Is(nameof(UserEntity.Username))
                        .EqualTo(_userIdOrName)
                    )
                    .RunAsync(cancellationToken);

                if (_createRequest is not null)
                {
                    if (userEntity is not null)
                        throw new UserAlreadyExistsError(userEntity);

                    userEntity = await _storagrService
                        .Database
                        .Insert(_createRequest)
                        .RunAsync(cancellationToken);
                }

                if (userEntity is null)
                    throw new UserNotFoundError();

                if (_updateRequest is not null)
                {
                    if (_updateRequest.ContainsKey("username"))
                        userEntity.Username = (string) _updateRequest["username"];

                    if (_updateRequest.ContainsKey("is_admin"))
                        userEntity.IsAdmin = (bool) _updateRequest["is_admin"];

                    if (_updateRequest.ContainsKey("is_enabled"))
                        userEntity.IsEnabled = (bool) _updateRequest["is_enabled"];

                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .RemoveMany(new[]
                            {
                                StoragrCaching.GetUserKey(userEntity.Id),
                                StoragrCaching.GetUserKey(userEntity.Username)
                            })
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Database
                            .Update(userEntity)
                            .RunAsync(cancellationToken)
                    );
                }

                if (_deleteRequest)
                {
                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .RemoveMany(new[]
                            {
                                StoragrCaching.GetUserKey(userEntity.Id),
                                StoragrCaching.GetUserKey(userEntity.Username)
                            })
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Database
                            .Delete(userEntity)
                            .RunAsync(cancellationToken)
                    );
                }
                else if (cachedUser is null)
                {
                    await Task.WhenAll(
                        _storagrService
                            .Cache
                            .Key<UserEntity>(StoragrCaching.GetUserKey(userEntity.Id))
                            .Set(userEntity)
                            .RunAsync(cancellationToken),
                        _storagrService
                            .Cache
                            .Key<UserEntity>(StoragrCaching.GetUserKey(userEntity.Username))
                            .Set(userEntity)
                            .RunAsync(cancellationToken)
                    );
                }
                
                return userEntity;
            }

            IStoragrParams<UserEntity, IUserParams> IStoragrCreatable<UserEntity, IUserParams>.Create()
            {
                _createRequest = new UserEntity()
                {
                    Id = StoragrHelper.UUID(),
                    Username = _userIdOrName,
                    IsEnabled = true,
                    IsAdmin = false
                };
                // TODO create backend

                return this;
            }

            IStoragrParams<UserEntity, IUserParams> IStoragrUpdatable<UserEntity, IUserParams>.Update()
            {
                _updateRequest = new Dictionary<string, object>();
                return this;
            }

            IStoragrRunner<UserEntity> IStoragrDeletable<UserEntity>.Delete(bool force)
            {
                _deleteRequest = true;
                return this;
            }

            IStoragrRunner<bool> IStoragrExistable.Exists() => this;

            bool IStoragrRunner<bool>.Run() => (this as IStoragrRunner<bool>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<bool> IStoragrRunner<bool>.RunAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await (this as IStoragrRunner<UserEntity>).RunAsync(cancellationToken);
                }
                catch (UserNotFoundError)
                {
                    return false;
                }

                return true;
            }

            IStoragrRunner<UserEntity> IStoragrParams<UserEntity, IUserParams>.With(
                Action<IUserParams> withParams)
            {
                withParams(this);
                return this;
            }

            IUserParams IUserParams.Id(string userId)
            {
                // skipping - cannot modifiy id
                return this;
            }

            IUserParams IUserParams.Username(string username)
            {
                if (_createRequest is not null)
                    _createRequest.Username = username;

                _updateRequest?.Add("username", username);
                return this;
            }

            IUserParams IUserParams.Password(string password)
            {
                throw new NotImplementedException();
            }

            IUserParams IUserParams.IsEnabled(bool isEnabled)
            {
                if (_createRequest is not null)
                    _createRequest.IsEnabled = isEnabled;

                _updateRequest?.Add("is_enabled", isEnabled);
                return this;
            }

            IUserParams IUserParams.IsAdmin(bool isAdmin)
            {
                if (_createRequest is not null)
                    _createRequest.IsAdmin = isAdmin;

                _updateRequest?.Add("is_admin", isAdmin);
                return this;
            }


        }

        private class UserList :
            IStoragrServiceUsers,
            IUserParams
        {
            private readonly StoragrService _storagrService;
            private readonly UserEntity _entity;

            private int _take, _skip;
            private string _skipUntil;


            public UserList(StoragrService storagrService)
            {
                _storagrService = storagrService;
                _entity = new UserEntity();
            }

            IEnumerable<UserEntity> IStoragrRunner<IEnumerable<UserEntity>>.Run() =>
                (this as IStoragrRunner<IEnumerable<UserEntity>>)
                .RunAsync()
                .RunSynchronouslyWithResult();

            async Task<IEnumerable<UserEntity>> IStoragrRunner<IEnumerable<UserEntity>>.RunAsync(
                CancellationToken cancellationToken)
            {
                var users = await _storagrService
                    .Database
                    .Many<UserEntity>()
                    .Where(filter => filter
                        .Is(nameof(UserEntity.IsEnabled))
                        .EqualTo(_entity.IsEnabled)
                        .And()
                        .Is(nameof(UserEntity.IsAdmin))
                        .EqualTo(_entity.IsEnabled)
                        .And()
                        .Clamp(pattern =>
                        {
                            if (_entity.Id is not null)
                            {
                                pattern
                                    .Is(nameof(UserEntity.Id))
                                    .Like(_entity.Id)
                                    .Or();
                            }

                            if (_entity.Username is not null)
                            {
                                pattern
                                    .Is(nameof(UserEntity.Username))
                                    .Like(_entity.Username)
                                    .Or();
                            }
                        })
                    )
                    .RunAsync(cancellationToken);

                return users
                    .Skip(_skip)
                    .SkipWhile(e => _skipUntil is not null && e.Id != _skipUntil)
                    .Take(_take)
                    .Select(v => (UserEntity) v);
            }

            IStoragrRunner<int> IStoragrCountable.Count() => _storagrService
                .Database
                .Count<UserEntity>();

            IStoragrEnumerable<UserEntity, IUserParams> IStoragrEnumerable<UserEntity, IUserParams>.Take(
                int count)
            {
                _take = count;
                return this;
            }

            IStoragrEnumerable<UserEntity, IUserParams> IStoragrEnumerable<UserEntity, IUserParams>.Skip(
                int count)
            {
                _skip = count;
                return this;
            }

            IStoragrEnumerable<UserEntity, IUserParams> IStoragrEnumerable<UserEntity, IUserParams>.SkipUntil(
                string cursor)
            {
                _skipUntil = cursor;
                return this;
            }

            IStoragrEnumerable<UserEntity, IUserParams> IStoragrEnumerable<UserEntity, IUserParams>.Where(
                Action<IUserParams> whereParams)
            {
                whereParams(this);
                return this;
            }

            IUserParams IUserParams.Id(string userId)
            {
                _entity.Id = userId;
                return this;
            }

            IUserParams IUserParams.Username(string username)
            {
                _entity.Username = username;
                return this;
            }

            IUserParams IUserParams.Password(string password)
            {
                // skipping - cannot filter with password
                return this;
            }

            IUserParams IUserParams.IsEnabled(bool isEnabled)
            {
                _entity.IsEnabled = isEnabled;
                return this;
            }

            IUserParams IUserParams.IsAdmin(bool isAdmin)
            {
                _entity.IsAdmin = isAdmin;
                return this;
            }
        }
    }
}