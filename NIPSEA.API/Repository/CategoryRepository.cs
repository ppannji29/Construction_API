using Dapper;
using Microsoft.Extensions.Caching.Memory;
using NIPSEA.API.Context;
using NIPSEA.API.Contracts;
using NIPSEA.API.Entity;
using System.Data;

namespace NIPSEA.API.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DBContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CategoryRepository> _logger;
        private readonly IConfiguration _configuration;

        public CategoryRepository(DBContext dbContext,
            ILogger<CategoryRepository> logger,
            IMemoryCache memoryCache,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        private async Task<IEnumerable<Category>> GetCategoryListing()
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.GetMaster_Category";
                return await connection.QueryAsync<Category>(
                    storedProcedure,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<int> CreateNewCategory(CategoryDto dto)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.InsertMaster_Category";
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

        public async Task<IEnumerable<Category>> GetCategory()
        {
            const string cacheKey = "Categories";
            if (!_memoryCache.TryGetValue(cacheKey, out IEnumerable<Category> categoryList))
            {
                // If not in cache, fetch data from the database
                categoryList = await GetCategoryListing();

                // Set cache options
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(31)
                };

                // Store the data in the cache
                _memoryCache.Set(cacheKey, categoryList, cacheOptions);
            }
            return categoryList;
        }

        public async Task<IEnumerable<Category>> RefreshCategory()
        {
            IEnumerable<Category> categoryList = await GetCategoryListing();
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(31)
            };

            const string cacheKey = "Categories";
            _memoryCache.Set(cacheKey, categoryList, cacheOptions);

            return categoryList;
        }
    }
}
