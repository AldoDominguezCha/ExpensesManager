using Dapper;
using ExpnesesManager.Models;
using Microsoft.Data.SqlClient;

namespace ExpnesesManager.Services
{

    public interface IUsersRepository
    {
        Task<int> CreateUser(User user);
        Task<User> GetUserByEmail(string normalizedEmail);
    }

    public class UsersRepository : IUsersRepository
    {

        private readonly string _connectionString;
        public UsersRepository(IConfiguration configuration)
        {

            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        public async Task<int> CreateUser(User user)
        {
            using var connection = new SqlConnection(_connectionString);

            int id = await connection.QuerySingleAsync<int>(
                @"
                    INSERT INTO [Users] (Email, NormalizedEmail, PasswordHash) VALUES(@Email, @NormalizedEmail, @PasswordHash);
                    SELECT SCOPE_IDENTITY();
                ", user);

            await connection.ExecuteAsync("PopulateNewUserData", new { userId = id }, commandType : System.Data.CommandType.StoredProcedure);
            
            return id;
        }

        public async Task<User> GetUserByEmail(string normalizedEmail)
        {

            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<User>(@"SELECT * FROM [Users] WHERE NormalizedEmail = @NormalizedEmail;", new { normalizedEmail });

        }

    }
}
