using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using NIPSEA.API.Entity;
using NIPSEA.API.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace NIPSEA.API.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Components.Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IAuthRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            IAuthRepository authRepository,
            ITokenService tokenService,
            ILogger<AuthController> logger
        )
        {
            _webHostEnvironment = webHostEnvironment;
            _authRepository = authRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ApiResponse> UserLogin([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Try to Login W/ Email");
                if (request.Email == "panji.admin@gmail.com")
                {

                    User findUser = await _authRepository.GetUser(request.Email);
                    (string accessToken, string refreshToken, DateTime refreshTokenExpiry) userToken = _tokenService.IssueTokens(findUser, HttpContext);

                    return new ApiResponse(new
                    {
                        token = userToken.accessToken,
                        user = findUser
                    });
                }

                User getUser = await _authRepository.GetUser(request.Email);
                (string accessToken, string refreshToken, DateTime refreshTokenExpiry) getToken = _tokenService.IssueTokens(getUser, HttpContext);

                return new ApiResponse(new
                {
                    token = getToken.accessToken,
                    user = getUser
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }

        [HttpGet("token/AuthRefresh")]
        public async Task<IActionResult> AuthRefresh()
        {
            try
            {
                HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken);
                HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

                //Validate token
                Guid token_userID = _tokenService.ValidateTokens(accessToken, refreshToken);

                // Get Payload from token
                var payload = _tokenService.GetPayloadFromToken(accessToken);

                // Extract/Get UserID and RoleID From Payload Value
                Guid userId = Guid.Parse(payload.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value);

                User user = await _authRepository.GetUserByUserID(userId);

                if (user == null)
                {
                    return BadRequest("UserID Not Found");
                }   

                (string accessToken, string refreshToken, DateTime refreshTokenExpiry) userToken = _tokenService.IssueTokens(user, HttpContext);
                return Ok();
                //return Ok(new
                //{
                //    refreshToken = userToken.refreshToken,
                //    Expiry = userToken.refreshTokenExpiry
                //});
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, ex.Message);
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpDelete("sign-out")]
        public async Task<ApiResponse> UserSignOut()
        {
            try
            {
                HttpContext.Request.Cookies.TryGetValue("accessToken", out var accessToken);
                HttpContext.Request.Cookies.TryGetValue("refreshToken", out var refreshToken);

                if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(accessToken))
                {
                    return new ApiResponse(401, "Unauthorized.");
                }

                Guid userID = Guid.Empty;

                // bypass validate token if log out stuck
                try
                {
                    userID = _tokenService.ValidateTokens(accessToken, refreshToken);
                }
                catch (SecurityTokenException ex)
                {
                    _logger.LogInformation(ex, "Validate token failed, ByPass Validate token to continue logout.");
                }

                CookieOptions tokenCookieOptions = new CookieOptions
                {
                    Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                };
                Response.Cookies.Append("accessToken", "", tokenCookieOptions);
                Response.Cookies.Append("refreshToken", "", tokenCookieOptions);

                Response.Cookies.Delete("accessTokenExpiry");
                Response.Cookies.Delete("refreshTokenExpiry");

                return new ApiResponse("You have been logged out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse(500, ex);
            }
        }
    }
}
