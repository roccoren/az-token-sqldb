using Azure.Core;
using Azure.Identity;
using AzureSqlTokenAuth.Configuration;
using Microsoft.Extensions.Options;

namespace AzureSqlTokenAuth.Auth;

public interface ITokenManager
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
    Task RefreshTokenAsync(CancellationToken cancellationToken = default);
}

public class TokenManager : ITokenManager
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static readonly string[] _scopes = new[] { "https://database.windows.net/.default" };

    private readonly ClientSecretCredential _credential;
    private readonly ILogger<TokenManager> _logger;
    private readonly IOptionsMonitor<AzureAdOptions> _azureAdOptions;
    private AccessToken? _currentToken;

    public TokenManager(
        IOptionsMonitor<AzureAdOptions> azureAdOptions,
        ILogger<TokenManager> logger)
    {
        _azureAdOptions = azureAdOptions;
        _logger = logger;

        _logger.LogInformation("Initializing TokenManager with TenantId: {TenantId}, ClientId: {ClientId}",
            _azureAdOptions.CurrentValue.TenantId,
            _azureAdOptions.CurrentValue.ClientId);

        try
        {
            _credential = new ClientSecretCredential(
                _azureAdOptions.CurrentValue.TenantId,
                _azureAdOptions.CurrentValue.ClientId,
                _azureAdOptions.CurrentValue.ClientSecret);

            _logger.LogInformation("Successfully created ClientSecretCredential");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create ClientSecretCredential. TenantId format may be invalid.");
            throw;
        }
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_currentToken is null || IsTokenExpired(_currentToken.Value))
        {
            await RefreshTokenAsync(cancellationToken);
        }

        return _currentToken?.Token ?? throw new InvalidOperationException("Failed to obtain token");
    }

    public async Task RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            // Double check after acquiring the lock
            if (_currentToken is null || IsTokenExpired(_currentToken.Value))
            {
                _logger.LogInformation("Refreshing Azure SQL Database access token");
                _currentToken = await _credential.GetTokenAsync(new TokenRequestContext(_scopes), cancellationToken);
                _logger.LogDebug("Token refreshed successfully. Expires at: {ExpiresOn}", _currentToken.Value.ExpiresOn);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh token");
            throw new TokenRefreshException("Failed to refresh Azure SQL Database access token", ex);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static bool IsTokenExpired(AccessToken token)
    {
        // Consider token expired if it expires in less than 5 minutes
        return token.ExpiresOn <= DateTimeOffset.UtcNow.AddMinutes(5);
    }
}

public class TokenRefreshException : Exception
{
    public TokenRefreshException(string message) : base(message) { }
    public TokenRefreshException(string message, Exception innerException) : base(message, innerException) { }
}