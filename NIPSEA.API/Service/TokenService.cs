using Azure;
using Azure.Core;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NIPSEA.API.Contracts;
using NIPSEA.API.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NIPSEA.API.Service
{
    public class TokenService : ITokenService
    {
        private readonly JWTSetting _jWTSetting;

        public TokenService(IOptions<JWTSetting> jwtSetting)
        {
            _jWTSetting = jwtSetting.Value;
        }

        private string GenerateAccessToken(IEnumerable<Claim> userClaims, DateTime expiryDateTime)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWTSetting.SecretKey));
            var signingCredentials_key = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var tokeOptions = new JwtSecurityToken(
                issuer: _jWTSetting.Issuer,
                audience: _jWTSetting.Audience,
                claims: userClaims,
                expires: expiryDateTime,
                signingCredentials: signingCredentials_key
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
            return tokenString;
        }

        private string GenerateRefreshToken(Guid userId, DateTime expiryDateTime, string iv)
        {
            var tokenData = $"{userId.ToString()}|{expiryDateTime:O}";
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_jWTSetting.RefreshTokenEncrptKey);
                aes.IV = Encoding.UTF8.GetBytes(iv);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(tokenData);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWTSetting.SecretKey)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                var jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch (Exception ex)
            {
                return new ClaimsPrincipal();
            }

        }




        public (Guid userId, DateTime expiry) DecryptToken(string encryptedToken, string iv)
        {
            try
            {
                var buffer = Convert.FromBase64String(encryptedToken);
                using (var aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(_jWTSetting.RefreshTokenEncrptKey);
                    aes.IV = Encoding.UTF8.GetBytes(iv);

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    using (var ms = new MemoryStream(buffer))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var sr = new StreamReader(cs))
                            {
                                var tokenData = sr.ReadToEnd();
                                var parts = tokenData.Split('|');
                                Guid userId = Guid.Parse(parts[0]);
                                var expiry = DateTime.Parse(parts[1], null, System.Globalization.DateTimeStyles.RoundtripKind);
                                return (userId, expiry);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }
        }


        public (string accessToken, string refreshToken, DateTime refreshTokenExpiry) IssueTokens(User user, HttpContext context)
        {
            //Step 0 Data Prep
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };
            DateTime accessTokenExpiry = DateTime.UtcNow.AddMinutes(_jWTSetting.TokenDurationInMin);
            DateTime refreshTokenExpiry = DateTime.UtcNow.AddHours(_jWTSetting.RefreshTokenDurationInHour);

            //Step 1 Gen accessToken
            var accessToken = GenerateAccessToken(claims, accessTokenExpiry);

            //Step 2 Gen refreshToken (IV from FIRST 16 char of accessToken)
            var refreshToken = GenerateRefreshToken(user.UserID, refreshTokenExpiry, accessToken.Substring(0, 16));

            //Step 3 Set token to httponly, and expiry to normal cookies
            CookieOptions tokenCookieOptions = new CookieOptions
            {
                Expires = refreshTokenExpiry,
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            context.Response.Cookies.Append("accessToken", accessToken, tokenCookieOptions);
            context.Response.Cookies.Append("refreshToken", refreshToken, tokenCookieOptions);

            CookieOptions normalCookieOptions = new CookieOptions
            {
                Expires = refreshTokenExpiry,
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            context.Response.Cookies.Append("accessTokenExpiry", accessTokenExpiry.ToString("o"), normalCookieOptions);
            context.Response.Cookies.Append("refreshTokenExpiry", refreshTokenExpiry.ToString("o"), normalCookieOptions);


            return (accessToken, refreshToken, refreshTokenExpiry);
        }


        public Guid ValidateTokens(string accessToken, string refreshToken)
        {
            //Step 1: Check if accessToken and refreshToken is there
            if (accessToken == null || refreshToken == null)
                throw new SecurityTokenException("Invalid tokens");

            //Step 2: GET userid in accessToken 
            var tokenClaim = GetPrincipalFromToken(accessToken);
            if (tokenClaim == null)
                throw new SecurityTokenException("Invalid access token claims");
            var userId = tokenClaim.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;

            //Step 3: Decrypt refreshToken
            (Guid userId, DateTime expiry) decryptedRefreshToken = DecryptToken(refreshToken, accessToken.Substring(0, 16));

            //Step 4: Verify expiry date
            if (decryptedRefreshToken.expiry <= DateTime.Now)
                throw new SecurityTokenException("Invalid expired token claims");

            //Step 5: Verify user id
            if (userId != decryptedRefreshToken.userId.ToString())
                throw new SecurityTokenException("Invalid token claims mismatched");

            return decryptedRefreshToken.userId;
        }

        public List<Claim> GetPayloadFromToken(string accessToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var readToken = tokenHandler.ReadJwtToken(accessToken);

            // get payload from token
            var claims = readToken.Claims.ToList();

            return claims;
        }
    }
}
