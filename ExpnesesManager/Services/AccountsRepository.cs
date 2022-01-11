using Dapper;
using ExpnesesManager.Models;
using Microsoft.Data.SqlClient;

namespace ExpnesesManager.Services
{

    public interface IAccountsRepository
    {
        Task Create(Account account);
        Task<IEnumerable<Account>> SearchAccounts(int userId);
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

    }
}
