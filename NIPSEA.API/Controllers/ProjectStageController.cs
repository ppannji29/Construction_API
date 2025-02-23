using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Mvc;
using NIPSEA.API.Contracts;

namespace NIPSEA.API.Controllers
{
    [ApiController]
    [Route("project-stage")]
    public class ProjectStageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IProjectStageRepository _projectStageRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;
        private readonly ITokenService _tokenService;

        public ProjectStageController(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            IProjectStageRepository projectStageRepository,
            ITokenService tokenService,
            ILogger<ProjectStageController> logger
        )
        {
            _webHostEnvironment = webHostEnvironment;
            _projectStageRepository = projectStageRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<ApiResponse> GetProjectStageColection()
        {
            try
            {
                return new ApiResponse(await _projectStageRepository.GetProjectStage());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }
    }
}
