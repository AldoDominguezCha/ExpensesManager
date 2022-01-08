using Dapper;
using ExpnesesManager.Models;
using Microsoft.Data.SqlClient;

namespace ExpnesesManager.Services
{

    public interface IAccountTypesRepository
    {
        Task<bool> AccountTypeExists(string name, int userId);
        Task Create(AccountType accountType);
        Task DeleteAccountType(int id);
        Task<AccountType> GetAccountTypeById(int id, int userId);
        Task<IEnumerable<AccountType>> GetAccountTypes(int userId);
        Task SetAccountTypesOrder(IEnumerable<AccountType> orderedAccountTypes);
        Task UpdateAccountType(AccountType accountType);
    }
    public class AccountTypesRepository : IAccountTypesRepository
    {

        private readonly string _connectionString;
        public AccountTypesRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Create(AccountType accountType)
        {
            using var connection = new SqlConnection(_connectionString);
            var Id = await connection.QuerySingleAsync<int>("AccountTypesInsert", new { UserId = accountType.UserId, Name = accountType.Name }, commandType : System.Data.CommandType.StoredProcedure);

            accountType.Id = Id;
        }

        public async Task<bool> AccountTypeExists(string name, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstOrDefaultAsync<int>("SELECT 1 FROM [AccountTypes] WHERE Name = @Name AND UserId = @UserId;", new { name, userId }) == 1;
        }

        public async Task<IEnumerable<AccountType>> GetAccountTypes(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<AccountType>(@"SELECT Id, Name, SortOrder FROM [AccountTypes] WHERE UserId = @UserId ORDER BY SortOrder ASC;", new { userId });
        }

        public async Task UpdateAccountType(AccountType accountType)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"UPDATE [AccountTypes] SET Name = @Name WHERE Id = @Id;", accountType);
        }

        public async Task<AccountType> GetAccountTypeById(int id, int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryFirstAsync<AccountType>("SELECT Id, Name, SortOrder FROM [AccountTypes] WHERE Id = @Id AND UserId = @UserId;", new { id, userId });

        }

        public async Task DeleteAccountType(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("DELETE FROM [AccountTypes] WHERE Id = @Id;", new { id });
        }

        public async Task SetAccountTypesOrder(IEnumerable<AccountType> orderedAccountTypes)
        {
            var query = "UPDATE [AccountTypes] SET SortOrder = @SortOrder WHERE Id = @Id;";
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(query, orderedAccountTypes);
        }

    }
}
