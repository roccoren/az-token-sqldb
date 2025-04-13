using AzureSqlTokenAuth.Data;
using AzureSqlTokenAuth.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AzureSqlTokenAuth.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<User?> GetUserAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(long id, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<UserService> _logger;

    public UserService(IConnectionManager connectionManager, ILogger<UserService> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO [users] ([name], [email])
            OUTPUT INSERTED.[id], INSERTED.[name], INSERTED.[email], INSERTED.[created_at], INSERTED.[updated_at]
            VALUES (@Name, @Email)";

        using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        using var command = new SqlCommand(sql, (SqlConnection)connection);
        
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Email", request.Email);

        try
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new User
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    UpdatedAt = reader.GetDateTime(4)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            throw;
        }

        throw new InvalidOperationException("Failed to create user - no data returned");
    }

    public async Task<User?> GetUserAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT [id], [name], [email], [created_at], [updated_at]
            FROM [users]
            WHERE [id] = @Id";

        using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        using var command = new SqlCommand(sql, (SqlConnection)connection);
        
        command.Parameters.AddWithValue("@Id", id);

        try
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new User
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    UpdatedAt = reader.GetDateTime(4)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user with ID: {UserId}", id);
            throw;
        }

        return null;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT [id], [name], [email], [created_at], [updated_at]
            FROM [users]";

        var users = new List<User>();
        using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        using var command = new SqlCommand(sql, (SqlConnection)connection);

        try
        {
            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                users.Add(new User
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Email = reader.GetString(2),
                    CreatedAt = reader.GetDateTime(3),
                    UpdatedAt = reader.GetDateTime(4)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            throw;
        }

        return users;
    }

    public async Task<bool> UpdateUserAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE [users]
            SET [name] = @Name,
                [email] = @Email
            WHERE [id] = @Id";

        using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        using var command = new SqlCommand(sql, (SqlConnection)connection);
        
        command.Parameters.AddWithValue("@Id", id);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Email", request.Email);

        try
        {
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with ID: {UserId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM [users] WHERE [id] = @Id";

        using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
        using var command = new SqlCommand(sql, (SqlConnection)connection);
        
        command.Parameters.AddWithValue("@Id", id);

        try
        {
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with ID: {UserId}", id);
            throw;
        }
    }
}