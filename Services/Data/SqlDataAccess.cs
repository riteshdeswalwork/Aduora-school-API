using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace sms.api.Services.Data
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly string _connectionString;

        public SqlDataAccess(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection connection string is not configured.");
        }

        private SqlConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<IReadOnlyList<T>> QueryStoredProcAsync<T>(string procedureName, object? parameters = null)
        {
            using var connection = CreateConnection();
            var rows = await connection.QueryAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
            return rows.AsList();
        }

        public async Task<T?> QuerySingleStoredProcAsync<T>(string procedureName, object? parameters = null)
        {
            using var connection = CreateConnection();
            return await connection.QuerySingleOrDefaultAsync<T>(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<(IReadOnlyList<T> Items, int TotalCount)> QueryPagedStoredProcAsync<T>(string procedureName, object? parameters = null)
        {
            using var connection = CreateConnection();
            using var multi = await connection.QueryMultipleAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
            var items = (await multi.ReadAsync<T>()).AsList();
            var totalCount = await multi.ReadSingleOrDefaultAsync<int>();
            return (items, totalCount);
        }

        public async Task<int> ExecuteStoredProcAsync(string procedureName, object? parameters = null)
        {
            using var connection = CreateConnection();
            return await connection.ExecuteAsync(procedureName, parameters, commandType: CommandType.StoredProcedure);
        }
    }
}
