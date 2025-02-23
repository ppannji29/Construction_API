using System.Data;
using Microsoft.Data.SqlClient;

namespace NIPSEA.API.Context
{
    public class DBContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DBContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqlConnection");
        }

        public IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public string ConnectionString => _connectionString;
    }
}
