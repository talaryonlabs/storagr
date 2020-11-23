namespace Storagr
{
    public class TokenData
    {
        public string UniqueId { get; set; }
        public string Role { get; set; }
    }
    
    public interface ITokenService
    {
        string Generate(TokenData data);
        string Refresh(string token);
        bool Verify(string token, TokenData data);

        TokenData Get(string token);
    }
}