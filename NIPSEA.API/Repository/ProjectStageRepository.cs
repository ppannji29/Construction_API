using Dapper;
using Microsoft.Extensions.Caching.Memory;
using NIPSEA.API.Context;
using NIPSEA.API.Contracts;
using NIPSEA.API.Entity;
using System.Data;

namespace NIPSEA.API.Repository
{
    public class ProjectStageRepository : IProjectStageRepository
    {
        private readonly DBContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ProjectStageRepository> _logger;
        private readonly IConfiguration _configuration;

        public ProjectStageRepository(DBContext dbContext,
            ILogger<ProjectStageRepository> logger,
            IMemoryCache memoryCache,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<int> CreateNewProjectStage(ProjectStageDto dto)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.InsertMaster_ProjectStage";
                return await connection.ExecuteScalarAsync<int>(
                    storedProcedure,
                    new
                    {
                        Name = dto.Name,
                        CreatedBy = dto.CreatedBy,
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<IEnumerable<ProjectStage>> GetProjectStage()
        {
            const string cacheKey = "ProjectStageListing";
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<ProjectStage> projectStage))
            {
                // If not in cache, fetch data from the database
                projectStage = await GetListingProjectStage();

                // Set cache options
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(31)
                };

                // Store the data in the cache
                _memoryCache.Set(cacheKey, projectStage, cacheOptions);
            }
            return projectStage;
        }

        public async Task<IEnumerable<ProjectStage>> RefreshProjectStage()
        {
            IEnumerable<ProjectStage> projectStage = await GetListingProjectStage();
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(31)
            };

            const string cacheKey = "ProjectStageListing";
            _memoryCache.Set(cacheKey, projectStage, cacheOptions);

            return projectStage;
        }

        private async Task<IEnumerable<ProjectStage>> GetListingProjectStage()
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.GetMaster_ProjectStage";
                return await connection.QueryAsync<ProjectStage>(
                    storedProcedure,
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}
