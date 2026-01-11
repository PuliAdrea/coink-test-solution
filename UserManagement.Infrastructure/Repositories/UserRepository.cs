using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public async Task<int> RegisterUserAsync(User user)
    {
        // We use the function created in the Database step: sp_RegisterUser
        var sql = "SELECT * FROM sp_RegisterUser(@Name, @Phone, @Address, @MunicipalityId)";

        var parameters = new
        {
            Name = user.Name,
            Phone = user.Phone,
            Address = user.Address,
            MunicipalityId = user.MunicipalityId
        };

        using var connection = new NpgsqlConnection(_connectionString);

        // QuerySingleAsync is used because the function returns one integer (the ID)
        var newId = await connection.QuerySingleAsync<int>(sql, parameters);

        return newId;
    }
    public async Task<IEnumerable<dynamic>> GetAllUsersAsync()
    {
        using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryAsync<dynamic>("SELECT * FROM sp_GetAllUsers()");
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        return await conn.QueryFirstOrDefaultAsync<User>("SELECT * FROM sp_GetUserById(@Id)", new { Id = id });
    }

    public async Task UpdateUserAsync(User user)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync("SELECT sp_UpdateUser(@Id, @Name, @Phone, @Address, @MunicipalityId)", user);
    }

    public async Task DeleteUserAsync(int id)
    {
        using var conn = new NpgsqlConnection(_connectionString);
        await conn.ExecuteAsync("SELECT sp_DeleteUser(@Id)", new { Id = id });
    }
}