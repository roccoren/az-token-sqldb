using AzureSqlTokenAuth.Auth;
using AzureSqlTokenAuth.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Data;

namespace AzureSqlTokenAuth.Data;

public sealed class ConnectionManager : IConnectionManager
{
    private const int MaxPoolSize = 100;
    private readonly ConcurrentQueue<SqlConnection> _connectionPool;
    private readonly SemaphoreSlim _poolSemaphore;
    private readonly ITokenManager _tokenManager;
    private readonly SqlDatabaseOptions _options;
    private readonly ILogger<ConnectionManager> _logger;
    private bool _disposed;

    public ConnectionManager(
        ITokenManager tokenManager,
        IOptions<SqlDatabaseOptions> options,
        ILogger<ConnectionManager> logger)
    {
        _tokenManager = tokenManager;
        _options = options.Value;
        _logger = logger;
        _connectionPool = new ConcurrentQueue<SqlConnection>();
        _poolSemaphore = new SemaphoreSlim(MaxPoolSize, MaxPoolSize);
    }

    public async Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ConnectionManager));
        }

        await _poolSemaphore.WaitAsync(cancellationToken);

        try
        {
            if (_connectionPool.TryDequeue(out SqlConnection? connection))
            {
                if (IsConnectionValid(connection))
                {
                    _logger.LogDebug("Reusing existing connection from pool");
                    return connection;
                }

                await DisposeSafelyAsync(connection);
            }

            return await CreateNewConnectionAsync(cancellationToken);
        }
        catch (Exception)
        {
            _poolSemaphore.Release();
            throw;
        }
    }

    private async Task<SqlConnection> CreateNewConnectionAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating new database connection");

        _logger.LogInformation("Attempting to connect to SQL Server with connection string: {ConnectionString}",
            _options.ConnectionString.Replace(_options.Database, "[MASKED-DB-NAME]")); // Mask sensitive parts but show server name

        var connection = new SqlConnection(_options.ConnectionString);

        try
        {
            // Get the access token and set it on the connection
            string token = await _tokenManager.GetTokenAsync(cancellationToken);
            connection.AccessToken = token;

            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch (Exception ex)
        {
            await DisposeSafelyAsync(connection);
            _logger.LogError(ex, "Failed to create database connection");
            throw;
        }
    }

    private static bool IsConnectionValid(SqlConnection connection)
    {
        return connection.State == ConnectionState.Open;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        while (_connectionPool.TryDequeue(out SqlConnection? connection))
        {
            connection.Dispose();
        }

        _poolSemaphore.Dispose();
    }

    private async Task DisposeSafelyAsync(SqlConnection? connection)
    {
        if (connection == null)
        {
            return;
        }

        try
        {
            await connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing connection");
        }
    }

    public async ValueTask ReleaseConnectionAsync(SqlConnection connection)
    {
        if (_disposed)
        {
            await DisposeSafelyAsync(connection);
            return;
        }

        if (IsConnectionValid(connection))
        {
            _connectionPool.Enqueue(connection);
        }
        else
        {
            await DisposeSafelyAsync(connection);
        }

        _poolSemaphore.Release();
    }
}