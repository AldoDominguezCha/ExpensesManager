using Dapper;
using ExpnesesManager.Models;
using Microsoft.Data.SqlClient;

namespace ExpnesesManager.Services
{

    public interface IAccountsRepository
    {
        Task Create(Account account);
        Task DeleteAccount(int id);
        Task<Account> GetAccountById(int id, int userId);
        Task<IEnumerable<Account>> SearchAccounts(int userId);
        Task UpdateAccount(CreateAccountViewModel account);
    }
    public class AccountsRepository : IAccountsRepository
    {
        private readonly string _connection;

        public AccountsRepository(IConfiguration configuration)
        {
            _connection = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task Create(Account account)
        {
            using var connection = new SqlConnection(_connection);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO [Accounts] (Name, AccountTypeId, AccountDescription, Balance) VALUES(@Name, @AccountTypeId, @AccountDescription, @Balance); SELECT SCOPE_IDENTITY();", account);

            account.Id = id;
        }

        public async Task<IEnumerable<Account>> SearchAccounts(int userId)
        {
            using var connection = new SqlConnection(_connection);
            return await connection.QueryAsync<Account>(@"SELECT Accounts.Id, Accounts.Name, Accounts.Balance, ac_t.Name AS AccountType
                                FROM [Accounts]
                                INNER JOIN AccountTypes ac_t
                                ON ac_t.Id = Accounts.AccountTypeId
                                WHERE ac_t.UserId = @UserId
                                ORDER BY ac_t.SortOrder;", new { userId });
        }

        public async Task<Account> GetAccountById(int id, int userId)
        {
            using var connection = new SqlConnection(_connection);

            return await connection.QueryFirstOrDefaultAsync<Account>(@"
                SELECT Accounts.Id, Accounts.Name, Accounts.Balance, AccountDescription, AccountTypeId
                                FROM [Accounts]
                                INNER JOIN AccountTypes ac_t
                                ON ac_t.Id = Accounts.AccountTypeId
                                WHERE ac_t.UserId = @UserId AND Accounts.Id = @Id", new { id, userId });
        }

        public async Task UpdateAccount(CreateAccountViewModel account)
        {
            using var connection = new SqlConnection(_connection);
            await connection.ExecuteAsync(@"
                UPDATE [Accounts]
                SET Name = @Name, Balance = @Balance, AccountDescription = @AccountDescription, AccountTypeId = @AccountTypeId
                WHERE Id = @Id;
            ", account);
        }

        public async Task DeleteAccount(int id)
        {
            using var connection = new SqlConnection(_connection);

            await connection.ExecuteAsync("DELETE [Accounts] WHERE Id = @Id", new { id }); 
        }

    }
}
