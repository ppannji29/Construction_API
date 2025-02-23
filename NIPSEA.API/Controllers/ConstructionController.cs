using AutoWrapper.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NIPSEA.API.Contracts;
using NIPSEA.API.Entity;
using NIPSEA.API.Repository;
using NIPSEA.API.Service;
using System.Configuration;
using System.Linq;

namespace NIPSEA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("construction")]
    public class ConstructionController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IProjectStageRepository _projectStageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IConstructionRepository _constructionRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger _logger;
        private readonly ITokenService _tokenService;

        public ConstructionController(
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            IProjectStageRepository projectStageRepository,
            ICategoryRepository categoryRepository,
            IConstructionRepository constructionRepository,
            ITokenService tokenService,
            ILogger<AuthController> logger
        ) {
            _webHostEnvironment = webHostEnvironment;
            _projectStageRepository = projectStageRepository;
            _categoryRepository = categoryRepository;
            _constructionRepository = constructionRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("")]
        public async Task<ApiResponse> CreateNewConstructionProject([FromBody] IEnumerable<CreateConstructionDto> dtoList)
        {
            try
            {
                IEnumerable<ProjectStage> projectStages = (await _projectStageRepository.GetProjectStage()).Where(ps => ps.Status == 1);
                // Get the current date
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);
                // Check valid constrcution start date
                bool invalidConstructionDate = dtoList.Any(dto => projectStages.Any(ps => ps.ProjectStageID == dto.ProjectStageID) && dto.ProjectStartDate < today);
                if (invalidConstructionDate)
                {
                    return new ApiResponse(new
                    {
                        success = false,
                        message = "Construction Start Date must be in the future for Concept, Design & Documentation, and Pre-Construction stages."
                    });
                }

                IEnumerable<Category> categoryList = await _categoryRepository.GetCategory();

                foreach (CreateConstructionDto dto in dtoList)
                {
                    var findCategory = categoryList.FirstOrDefault(cat => cat.CategoryID == dto.CategoryID);

                    if (findCategory != null) { 
                        dto.CategoryID = findCategory.CategoryID;
                    } else
                    {
                        CategoryDto categoryDto = new CategoryDto
                        {
                            Name = dto.CategoryName,
                            CreatedBy = dto.CreatedBy
                        };

                        int categoryId = await _categoryRepository.CreateNewCategory(categoryDto);

                        dto.CategoryID = categoryId;
                    }
                }

                // insert batch construction
                await _constructionRepository.InsertNewConstruction(dtoList);

                return new ApiResponse(new
                {
                    success = true,
                    message = "Success to create new Project."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }

        [HttpPost("listing")]
        public async Task<ApiResponse> GetConstructionListing(FilterListing filter)
        {
            try
            {
                return new ApiResponse(await _constructionRepository.GetConstructionListingFilterByPackage(filter));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }

        [HttpDelete("{ProjectID}")]
        public async Task<ApiResponse> DeleteProjectByProjectID(int ProjectID)
        {
            try
            {
                return new ApiResponse(await _constructionRepository.DeleteConstructionByProjectID(ProjectID));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }

        [HttpPut("")]
        public async Task<ApiResponse> UpdateConstrcution(UpdConstructionDto updateDto)
        {
            try
            {
                IEnumerable<ProjectStage> projectStages = (await _projectStageRepository.GetProjectStage()).Where(ps => ps.Status == 1);
                // Get the current date
                DateOnly today = DateOnly.FromDateTime(DateTime.Today);

                bool invalidConstructionDate = projectStages.Any(ps => ps.ProjectStageID == updateDto.ProjectStageID) && updateDto.ProjectStartDate < today;
                if (invalidConstructionDate)
                {
                    return new ApiResponse(new
                    {
                        success = false,
                        message = "Construction Start Date must be in the future for Concept, Design & Documentation, and Pre-Construction stages."
                    });
                }

                IEnumerable<Category> categoryList = await _categoryRepository.GetCategory();
                var findCategory = categoryList.FirstOrDefault(cat => cat.CategoryID == updateDto.CategoryID);
                if (findCategory != null)
                {
                    updateDto.CategoryID = findCategory.CategoryID;
                } else
                {
                    CategoryDto categoryDto = new CategoryDto
                    {
                        Name = updateDto.CategoryName,
                        CreatedBy = updateDto.ModifiedBy
                    };

                    int categoryId = await _categoryRepository.CreateNewCategory(categoryDto);

                    updateDto.CategoryID = categoryId;
                }

                await _constructionRepository.UpdateNewConstruction(updateDto);

                return new ApiResponse(new
                {
                    success = true,
                    message = "Update successful."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }

        [HttpGet("detail/{ProjectID}")]
        public async Task<ApiResponse> GetProjectDetails(int ProjectID)
        {
            try
            {
                return new ApiResponse(await _constructionRepository.GetConstructionDetails(ProjectID));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return new ApiResponse("Please contact Administrator.");
            }
        }
    }
}
