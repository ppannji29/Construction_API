using NIPSEA.API.Entity;
using System.Security.Claims;

namespace NIPSEA.API.Contracts
{
    public interface ITokenService
    {
        public (string accessToken, string refreshToken, DateTime refreshTokenExpiry) IssueTokens(User user, HttpContext context);
        public Guid ValidateTokens(string accessToken, string refreshToken);
        public List<Claim> GetPayloadFromToken(string accessToken);
    }
}
