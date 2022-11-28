using SongTrade.Models;

namespace SongTrade.Auth
{
    public interface IAuthService
    {
        public string CreateToken(User user);

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);

        public bool ValidateCurrentToken(string token);

        public string GetClaim(string token, string claimType);
    }
}
