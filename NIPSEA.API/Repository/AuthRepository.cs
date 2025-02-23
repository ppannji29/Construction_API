using Dapper;
using Microsoft.Extensions.Options;
using NIPSEA.API.Context;
using NIPSEA.API.Contracts;
using NIPSEA.API.Entity;
using System.Data;

namespace NIPSEA.API.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DBContext _dbContext;
        private readonly ILogger<AuthRepository> _logger;
        private readonly IConfiguration _configuration;

        public AuthRepository(DBContext dbContext,
            ILogger<AuthRepository> logger, 
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IEnumerable<User>> GetAuth()
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.GetMaster_User";
                return await connection.QueryAsync<User>(
                    storedProcedure,
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<User> GetUser(string Email)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.GetMaster_UserByEmail";
                return await connection.QueryFirstOrDefaultAsync<User>(
                    storedProcedure,
                    new
                    {
                        Email
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }

        public async Task<User> GetUserByUserID(Guid UserID)
        {
            using (var connection = _dbContext.CreateConnection())
            {
                const string storedProcedure = "dbo.GetMaster_UserByUserID";
                return await connection.QueryFirstOrDefaultAsync<User>(
                    storedProcedure,
                    new
                    {
                        UserID
                    },
                    commandType: CommandType.StoredProcedure
                );
            }
        }
    }
}
