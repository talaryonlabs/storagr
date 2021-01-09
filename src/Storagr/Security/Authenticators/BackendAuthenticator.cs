using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Storagr.Data;
using Storagr.Data.Entities;
using Storagr.Shared;

namespace Storagr.Security.Authenticators
{
    public class BackendAuthenticator : IAuthenticationAdapter
    {
        [Table("BackendAuth")]
        private class Entity : IAuthenticationResult
        {
            [ExplicitKey] public string Id { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }
        
        public string Name => "storagr-backend";

        private readonly IDatabaseAdapter _database;

        public BackendAuthenticator(IDatabaseAdapter database)
        {
            _database = database;
        }

        public Task<IAuthenticationResult> Authenticate(string token, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task<IAuthenticationResult> Authenticate(string username, string password, CancellationToken cancellationToken)
        {
            var backendUser = await _database.Get<Entity>(filter =>
            {
                filter
                    .Equal(nameof(Entity.Username), username);
            }, cancellationToken);
            if (backendUser is null)
                return null;

            return new PasswordHasher<Entity>()
                .VerifyHashedPassword(backendUser, backendUser.Password, password) == PasswordVerificationResult.Success
                ? backendUser
                : null;
        }

        public async Task<IAuthenticationResult> Create([NotNull] string username, [NotNull] string password)
        {
            var entity = new Entity()
            {
                Id = StoragrHelper.UUID(),
                Username = username,
                Password = new PasswordHasher<Entity>().HashPassword(null, password),
            };
            await _database.Insert(entity);

            return entity;
        }

        public async Task Modify([NotNull] string id, [AllowNull] string username, [AllowNull] string password)
        {
            var user = await _database.Get<Entity>(q => q.Where(f => f.Equal(nameof(Entity.Id), id)));
            var newPassword = (password is not null ? new PasswordHasher<Entity>().HashPassword(null, password) : null);
            
            await _database.Update(new Entity()
            {
                Id = user.Id,
                Username = username ?? user.Username,
                Password = newPassword ?? user.Password,
            });
        }

        public Task Delete([NotNull] string authId, CancellationToken cancellationToken) =>
            _database.Delete(new Entity() {Id = authId}, cancellationToken);
    }
}