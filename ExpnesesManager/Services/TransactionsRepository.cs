using Dapper;
using ExpnesesManager.Models;
using Microsoft.Data.SqlClient;

namespace ExpnesesManager.Services
{

    public interface ITransactionsRepository
    {
        Task Create(Transaction transaction);
        Task DeleteTransaction(int id);
        Task<Transaction> GetTransactionById(int id, int userId);
        Task<IEnumerable<Transaction>> ObtainTransactionsByAccountId(ObtainTransactionsByAccount model);
        Task<IEnumerable<ObtainByMonthResult>> ObtainTransactionsByMonth(int userId, int year);
        Task<IEnumerable<Transaction>> ObtainTransactionsByUserId(GetTransactionsByUserParameter model);
        Task<IEnumerable<ObtainByWeekResult>> ObtainTransactionsByWeek(GetTransactionsByUserParameter model);
        Task Update(Transaction transaction, decimal previousAmount, int previousAccountId);
    }


    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly string _connectionString;
        public TransactionsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Create(Transaction transaction)
        {
            using var connection = new SqlConnection(_connectionString);

            int id = await connection.QuerySingleAsync<int>("TransactionsInsert", 
                new { 
                    transaction.UserId, 
                    transaction.OperationDate, 
                    transaction.Amount,
                    transaction.CategoryId, 
                    transaction.AccountId, 
                    transaction.Description 
                }, 
                commandType: System.Data.CommandType.StoredProcedure );

            transaction.Id = id;

        }

        public async Task<IEnumerable<Transaction>> ObtainTransactionsByUserId(GetTransactionsByUserParameter model)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Transaction>(
                @"
                    SELECT t.Id, t.Amount, t.OperationDate, c.Name AS Category, a.Name AS Account, c.OperationTypeId 
                    FROM [Transactions] t
                    INNER JOIN [Categories] c
                    ON t.CategoryId = c.Id
                    INNER JOIN [Accounts] a
                    ON t.AccountId = a.Id
                    WHERE t.UserId = @UserId
                    AND OperationDate BETWEEN @StartDate AND @EndDate ORDER BY t.OperationDate DESC;
                ", model);


        }

        public async Task<IEnumerable<Transaction>> ObtainTransactionsByAccountId(ObtainTransactionsByAccount model)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Transaction>(
                @"
                    SELECT t.Id, t.Amount, t.OperationDate, c.Name AS Category, a.Name AS Account, c.OperationTypeId 
                    FROM [Transactions] t
                    INNER JOIN [Categories] c
                    ON t.CategoryId = c.Id
                    INNER JOIN [Accounts] a
                    ON t.AccountId = a.Id
                    WHERE t.AccountId = @AccountId AND t.UserId = @UserId
                    AND OperationDate BETWEEN @StartDate AND @EndDate;
                ", model);

            
        }

        public async Task Update(Transaction transaction, decimal previousAmount, int previousAccountId)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync("TransactionUpdate",
                new
                {
                    transaction.Id,
                    transaction.OperationDate,
                    transaction.Amount,
                    transaction.CategoryId,
                    transaction.AccountId,
                    transaction.Description,
                    previousAmount,
                    previousAccountId
                },
                commandType: System.Data.CommandType.StoredProcedure);

        }

        public async Task<Transaction> GetTransactionById(int id, int userId)
        {

            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Transaction>
                (@"
                    SELECT * FROM [Transactions] 
                    INNER JOIN Categories cat
                    ON cat.Id = Transactions.CategoryId
                    WHERE Transactions.Id = @Id AND Transactions.UserId = @UserId;", 
                new { id, userId });

        }

        public async Task<IEnumerable<ObtainByWeekResult>> ObtainTransactionsByWeek(GetTransactionsByUserParameter model)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<ObtainByWeekResult>(
                @"
                    SELECT DATEDIFF(d, @StartDate, OperationDate) / 7 + 1 AS Week, SUM(Amount) AS Amount, cat.OperationTypeId
                    FROM [Transactions]
                    INNER JOIN [Categories] cat
                    ON Transactions.CategoryId = cat.Id
                    WHERE Transactions.UserId = @UserId AND
                    OperationDate BETWEEN @StartDate AND @EndDate
                    GROUP BY DATEDIFF(d, @StartDate, OperationDate) / 7, cat.OperationTypeId;
                ", model);
        }

        public async Task<IEnumerable<ObtainByMonthResult>> ObtainTransactionsByMonth(int userId, int year)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<ObtainByMonthResult>(
                @"
                    SELECT MONTH(OperationDate) AS Month, SUM(Amount) AS Amount, cat.OperationTypeId
                    FROM [Transactions]
                    INNER JOIN [Categories] cat
                    ON cat.Id = Transactions.CategoryId
                    WHERE Transactions.UserId = @UserId AND YEAR(OperationDate) = @Year
                    GROUP BY MONTH(OperationDate), cat.OperationTypeId;
                ", new { userId, year });
        }

        public async Task DeleteTransaction(int id)
        {
            using var connection = new SqlConnection( _connectionString);
            await connection.ExecuteAsync("TransactionsDelete", new { id }, commandType: System.Data.CommandType.StoredProcedure);

        }

    }
}
