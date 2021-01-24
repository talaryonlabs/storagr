using System;

namespace Storagr.Server
{
    public interface ITokenService
    {
        string Generate<T>(T token) where T : class; 
        string Generate<T>(T token, TimeSpan expiresIn) where T : class;
        
        // string Refresh(string token);
        // bool Verify<T>(string encodedToken, T token) where T : class;

        public T Get<T>(string encodedToken) where T : class, new();
    }
}