﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        private readonly IBackendAdapter _backend;

        public BackendAuthenticator(IBackendAdapter backend)
        {
            _backend = backend;
        }

        public Task<IAuthenticationResult> Authenticate(string token)
        {
            throw new NotSupportedException();
        }

        public async Task<IAuthenticationResult> Authenticate(string username, string password)
        {
            var backendUser = await _backend.Get<Entity>(q => q.Where(f => f.Equal(nameof(Entity.Username), username)));
            if (backendUser == null)
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
            await _backend.Insert(entity);

            return entity;
        }

        public async Task Modify([NotNull] string id, [AllowNull] string username, [AllowNull] string password)
        {
            var user = await _backend.Get<Entity>(q => q.Where(f => f.Equal(nameof(Entity.Id), id)));
            var newPassword = (password != null ? new PasswordHasher<Entity>().HashPassword(null, password) : null);
            
            await _backend.Update(new Entity()
            {
                Id = user.Id,
                Username = username ?? user.Username,
                Password = newPassword ?? user.Password,
            });
        }

        public async Task Delete([NotNull] string authId)
        {
            await _backend.Delete(new Entity() {Id = authId});
        }
    }
}