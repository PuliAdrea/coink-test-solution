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
}