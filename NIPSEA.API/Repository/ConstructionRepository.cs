using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using MySqlX.XDevAPI.Common;
using NIPSEA.API.Context;
using NIPSEA.API.Contracts;
using NIPSEA.API.Entity;
using System.Data;

namespace NIPSEA.API.Repository
{
    public class ConstructionRepository : IConstructionRepository
    {
        private readonly DBContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ConstructionRepository> _logger;
        private readonly IConfiguration _configuration;
        public ConstructionRepository(
            DBContext dbContext,
            ILogger<ConstructionRepository> logger,
            IMemoryCache memoryCache,
            IConfiguration configuration
        ) {
            _dbContext = dbContext;
            _logger = logger;
            _memoryCache = memoryCache;
            _configuration = configuration;
        }

        public async Task<bool> DeleteConstructionByProjectID(int ProjectID)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.DeleteTransaction_ConstrcutionByProjectID";
                int result = await connection.ExecuteScalarAsync<int>(
                    storedProcedure,
                    new
                    {
                        ProjectID
                    },
                    commandType: CommandType.StoredProcedure
                );

                return result == 1;
            }
        }

        public async Task<Construction> GetConstructionDetails(int ProjectID)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.GetTransaction_ConstructionDetail";
                return await connection.QueryFirstOrDefaultAsync<Construction>(
                    storedProcedure,
                    new
                    {
                        ProjectID
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<IEnumerable<Construction>> GetConstructionListingFilterByPackage(FilterListing filter)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.GetTransaction_ConstructionFilterByPackage";
                return await connection.QueryAsync<Construction>(
                    storedProcedure,
                    new
                    {
                        ProjectName = filter.ProjectName,
                        CategoryID = filter.CategoryID,
                        ProjectStageID = filter.ProjectStageID,
                        ProjectStartDate = filter.ProjectStartDate,
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<bool> InsertNewConstruction(IEnumerable<CreateConstructionDto> ConsDto)
        {
            using (IDbConnection connection = _dbContext.CreateConnection())
            {
                connection.Open();
                DataTable NewConstruction = new DataTable();
                NewConstruction.Columns.Add("ProjectName", typeof(string));
                NewConstruction.Columns.Add("ProjectStageID", typeof(int));
                NewConstruction.Columns.Add("CategoryID", typeof(int));
                NewConstruction.Columns.Add("StartDate", typeof(DateOnly));
                NewConstruction.Columns.Add("Description", typeof(string));
                NewConstruction.Columns.Add("CreatedBy", typeof(Guid));
                NewConstruction.Columns.Add("CreatedDate", typeof(DateTime));

                foreach (CreateConstructionDto data in ConsDto)
                {
                    NewConstruction.Rows.Add(
                        data.ProjectName,
                        data.ProjectStageID,
                        data.CategoryID,
                        data.ProjectStartDate,
                        data.Description,
                        data.CreatedBy,
                        DateTime.UtcNow
                    );
                }

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy((SqlConnection)connection))
                {
                    bulkCopy.DestinationTableName = "dbo.Transaction_Construction";

                    bulkCopy.ColumnMappings.Add("ProjectName", "ProjectName");
                    bulkCopy.ColumnMappings.Add("ProjectStageID", "ProjectStageID");
                    bulkCopy.ColumnMappings.Add("CategoryID", "CategoryID");
                    bulkCopy.ColumnMappings.Add("StartDate", "StartDate");
                    bulkCopy.ColumnMappings.Add("Description", "Description");
                    bulkCopy.ColumnMappings.Add("CreatedBy", "CreatedBy");
                    bulkCopy.ColumnMappings.Add("CreatedDate", "CreatedDate");

                    await bulkCopy.WriteToServerAsync(NewConstruction);
                }

                return true;
            }
        }

        public async Task<bool> UpdateNewConstruction(UpdConstructionDto constructionDto)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.UpdateTransaction_Construction";

                int result = await connection.ExecuteScalarAsync<int>(
                    storedProcedure,
                    new
                    {
                        ProjectID = constructionDto.ProjectID,
                        ProjectName = constructionDto.ProjectName,
                        ProjectStageID = constructionDto.ProjectStageID,
                        CategoryID = constructionDto.CategoryID,
                        ProjectStartDate = constructionDto.ProjectStartDate,
                        Description = constructionDto.Description,
                        ModifiedBy = constructionDto.ModifiedBy
                    },
                    commandType: CommandType.StoredProcedure
                );

                return result == 1;
            }
        }
    }
}
