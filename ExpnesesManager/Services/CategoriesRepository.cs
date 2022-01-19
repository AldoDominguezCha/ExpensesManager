using Dapper;
using ExpnesesManager.Models;
using Microsoft.Data.SqlClient;

namespace ExpnesesManager.Services
{

    public interface ICategoriesRepository
    {
        Task CreateCategory(Category category);
        Task DeleteCategoryById(int id);
        Task<IEnumerable<Category>> GetCategoriesForUser(int userId);
        Task<IEnumerable<Category>> GetCategoriesForUserByOperationType(int userId, OperationType operationTypeId);
        Task<Category> GetCategoryById(int id, int userId);
        Task UpdateCategory(Category category);
    }

    public class CategoriesRepository : ICategoriesRepository
    {

        public readonly string _connectionString;
        public readonly IUsersService _usersService;
        public CategoriesRepository(IConfiguration configuration, IUsersService usersService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _usersService = usersService;

        }

        public async Task CreateCategory(Category category)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>(
                @"INSERT INTO [Categories] (Name, OperationTypeId, UserId)
                VALUES(@Name, @OperationTypeId, @UserId);
                SELECT SCOPE_IDENTITY();", category);

            category.Id = id;
        }

        public async Task<IEnumerable<Category>> GetCategoriesForUser(int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Category>(@"SELECT * FROM [Categories] WHERE UserId = @UserId", new { userId });
        }

        public async Task<IEnumerable<Category>> GetCategoriesForUserByOperationType(int userId, OperationType operationTypeId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Category>(@"SELECT * FROM [Categories] WHERE UserId = @UserId AND OperationTypeId = @operationTypeId", new { userId, operationTypeId });
        }



        public async Task<Category> GetCategoryById(int id, int userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<Category>(
                @"SELECT * FROM [Categories] WHERE Id = @Id AND UserId = @UserId", new { id, userId });
        }

        public async Task UpdateCategory(Category category)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                @"UPDATE [Categories] SET Name = @Name, OperationTypeId = @OperationTypeId WHERE Id = @Id", category);
        }

        public async Task DeleteCategoryById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(
                @"DELETE [Categories] WHERE Id = @Id", new { id });
        }


    }
}
