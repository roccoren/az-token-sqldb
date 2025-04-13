namespace AzureSqlTokenAuth.Configuration;

public class AzureAdOptions
{
    public const string SectionName = "AzureAd";

    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

public class SqlDatabaseOptions
{
    public const string SectionName = "SqlDatabase";

    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public int TokenRefreshIntervalMinutes { get; set; } = 45;

    public string ConnectionString => 
        $"Server={Server}.database.windows.net;Database={Database};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
}