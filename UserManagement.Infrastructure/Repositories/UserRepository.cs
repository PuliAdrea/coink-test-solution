using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Diagnostics;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Interfaces;

namespace UserManagement.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;

        _logger.LogDebug("UserRepository initialized with connection string to database: {Database}",
            GetDatabaseName());
    }

    public async Task<int> RegisterUserAsync(User user)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug(
            "Executing stored procedure: sp_RegisterUser. Parameters: Name={Name}, Phone={Phone}, Address={Address}, MunicipalityId={MunicipalityId}",
            user.Name, user.Phone, user.Address, user.MunicipalityId);

        try
        {
            var sql = "SELECT * FROM sp_RegisterUser(@Name, @Phone, @Address, @MunicipalityId)";

            var parameters = new
            {
                Name = user.Name,
                Phone = user.Phone,
                Address = user.Address,
                MunicipalityId = user.MunicipalityId
            };

            using var connection = new NpgsqlConnection(_connectionString);

            _logger.LogDebug("Opening database connection...");
            await connection.OpenAsync();

            var newId = await connection.QuerySingleAsync<int>(sql, parameters);

            stopwatch.Stop();

            _logger.LogInformation(
                "✅ Database operation successful: sp_RegisterUser completed. NewUserId: {UserId}, ExecutionTime: {ExecutionTime}ms",
                newId, stopwatch.ElapsedMilliseconds);

            return newId;
        }
        catch (PostgresException pgEx)
        {
            stopwatch.Stop();

            _logger.LogError(pgEx,
                "❌ PostgreSQL error executing sp_RegisterUser. Code: {ErrorCode}, Message: {ErrorMessage}, ExecutionTime: {ExecutionTime}ms",
                pgEx.SqlState, pgEx.MessageText, stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "❌ Unexpected error executing sp_RegisterUser. ExecutionTime: {ExecutionTime}ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<IEnumerable<dynamic>> GetAllUsersAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("Executing stored procedure: sp_GetAllUsers");

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            var result = await connection.QueryAsync<dynamic>("SELECT * FROM sp_GetAllUsers()");

            stopwatch.Stop();

            var userCount = result?.Count() ?? 0;

            _logger.LogInformation(
                "✅ Database operation successful: sp_GetAllUsers completed. RecordsRetrieved: {RecordCount}, ExecutionTime: {ExecutionTime}ms",
                userCount, stopwatch.ElapsedMilliseconds);

            return result ?? Enumerable.Empty<dynamic>();
        }
        catch (PostgresException pgEx)
        {
            stopwatch.Stop();

            _logger.LogError(pgEx,
                "❌ PostgreSQL error executing sp_GetAllUsers. Code: {ErrorCode}, Message: {ErrorMessage}, ExecutionTime: {ExecutionTime}ms",
                pgEx.SqlState, pgEx.MessageText, stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "❌ Unexpected error executing sp_GetAllUsers. ExecutionTime: {ExecutionTime}ms",
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("Executing stored procedure: sp_GetUserById. Parameters: Id={UserId}", id);

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            var user = await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM sp_GetUserById(@Id)",
                new { Id = id });

            stopwatch.Stop();

            if (user != null)
            {
                _logger.LogInformation(
                    "✅ Database operation successful: sp_GetUserById completed. UserId: {UserId}, UserFound: true, ExecutionTime: {ExecutionTime}ms",
                    id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Database operation completed: sp_GetUserById - User not found. UserId: {UserId}, ExecutionTime: {ExecutionTime}ms",
                    id, stopwatch.ElapsedMilliseconds);
            }

            return user;
        }
        catch (PostgresException pgEx)
        {
            stopwatch.Stop();

            _logger.LogError(pgEx,
                "❌ PostgreSQL error executing sp_GetUserById. UserId: {UserId}, Code: {ErrorCode}, Message: {ErrorMessage}, ExecutionTime: {ExecutionTime}ms",
                id, pgEx.SqlState, pgEx.MessageText, stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "❌ Unexpected error executing sp_GetUserById. UserId: {UserId}, ExecutionTime: {ExecutionTime}ms",
                id, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug(
            "Executing stored procedure: sp_UpdateUser. Parameters: Id={UserId}, Name={Name}, Phone={Phone}, Address={Address}, MunicipalityId={MunicipalityId}",
            user.Id, user.Name, user.Phone, user.Address, user.MunicipalityId);

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "SELECT sp_UpdateUser(@Id, @Name, @Phone, @Address, @MunicipalityId)",
                user);

            stopwatch.Stop();

            _logger.LogInformation(
                "✅ Database operation successful: sp_UpdateUser completed. UserId: {UserId}, ExecutionTime: {ExecutionTime}ms",
                user.Id, stopwatch.ElapsedMilliseconds);
        }
        catch (PostgresException pgEx)
        {
            stopwatch.Stop();

            _logger.LogError(pgEx,
                "❌ PostgreSQL error executing sp_UpdateUser. UserId: {UserId}, Code: {ErrorCode}, Message: {ErrorMessage}, ExecutionTime: {ExecutionTime}ms",
                user.Id, pgEx.SqlState, pgEx.MessageText, stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "❌ Unexpected error executing sp_UpdateUser. UserId: {UserId}, ExecutionTime: {ExecutionTime}ms",
                user.Id, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task DeleteUserAsync(int id)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogDebug("Executing stored procedure: sp_DeleteUser. Parameters: Id={UserId}", id);

        try
        {
            using var connection = new NpgsqlConnection(_connectionString);

            await connection.OpenAsync();

            await connection.ExecuteAsync(
                "SELECT sp_DeleteUser(@Id)",
                new { Id = id });

            stopwatch.Stop();

            _logger.LogInformation(
                "✅ Database operation successful: sp_DeleteUser completed. UserId: {UserId}, ExecutionTime: {ExecutionTime}ms",
                id, stopwatch.ElapsedMilliseconds);
        }
        catch (PostgresException pgEx)
        {
            stopwatch.Stop();

            _logger.LogError(pgEx,
                "❌ PostgreSQL error executing sp_DeleteUser. UserId: {UserId}, Code: {ErrorCode}, Message: {ErrorMessage}, ExecutionTime: {ExecutionTime}ms",
                id, pgEx.SqlState, pgEx.MessageText, stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex,
                "❌ Unexpected error executing sp_DeleteUser. UserId: {UserId}, ExecutionTime: {ExecutionTime}ms",
                id, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    private string GetDatabaseName()
    {
        try
        {
            var builder = new NpgsqlConnectionStringBuilder(_connectionString);
            return builder.Database ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}