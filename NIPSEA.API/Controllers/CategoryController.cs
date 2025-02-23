using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NIPSEA.API.Contracts;

namespace NIPSEA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("category")]
    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;
        private readonly ITokenService _tokenService;

        public CategoryController(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ICategoryRepository categoryRepository,
            ITokenService tokenService,
            ILogger<CategoryController> logger
        )
        {
            _webHostEnvironment = webHostEnvironment;
            _categoryRepository = categoryRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<ApiResponse> GetCollectionCategory()
        {
            try
            {
                return new ApiResponse(await _categoryRepository.GetCategory());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }
    }
}
