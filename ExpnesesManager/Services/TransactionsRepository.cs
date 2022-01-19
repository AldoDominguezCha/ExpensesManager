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

        public async Task DeleteTransaction(int id)
        {
            using var connection = new SqlConnection( _connectionString);
            await connection.ExecuteAsync("TransactionsDelete", new { id }, commandType: System.Data.CommandType.StoredProcedure);

        }

    }
}
