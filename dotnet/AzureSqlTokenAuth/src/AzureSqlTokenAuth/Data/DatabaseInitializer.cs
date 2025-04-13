using System.IO;
using Microsoft.Data.SqlClient;

namespace AzureSqlTokenAuth.Data;

public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly IConnectionManager _connectionManager;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly Random _random = new Random();
    private readonly string[] _firstNames = new[] { "John", "Jane", "Bob", "Alice", "Charlie", "Diana", "Edward", "Fiona" };
    private readonly string[] _lastNames = new[] { "Smith", "Doe", "Johnson", "Brown", "Wilson", "Davis", "Moore", "Taylor" };

    public DatabaseInitializer(
        IConnectionManager connectionManager,
        ILogger<DatabaseInitializer> logger)
    {
        _connectionManager = connectionManager;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting database initialization...");
            string scriptPath = Path.Combine(AppContext.BaseDirectory, "Data", "Migrations", "001_CreateUsersTable.sql");

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Migration script not found at: {scriptPath}");
            }

            _logger.LogInformation("Reading migration script from: {ScriptPath}", scriptPath);
            string migrationScript = await File.ReadAllTextAsync(scriptPath, cancellationToken);

            // Execute the migration
            using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
            using var command = new SqlCommand(migrationScript, (SqlConnection)connection);

            await command.ExecuteNonQueryAsync(cancellationToken);

            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing database");
            throw;
        }
    }

    public async Task SeedSampleDataAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting to seed {Count} sample users...", count);

            using var connection = await _connectionManager.GetConnectionAsync(cancellationToken);
            for (int i = 0; i < count; i++)
            {
                var firstName = _firstNames[_random.Next(_firstNames.Length)];
                var lastName = _lastNames[_random.Next(_lastNames.Length)];
                var name = $"{firstName} {lastName}";
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}{_random.Next(100)}@example.com";

                var insertSql = @"
                    INSERT INTO [dbo].[users] ([name], [email])
                    VALUES (@Name, @Email);";

                using var command = new SqlCommand(insertSql, (SqlConnection)connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);

                await command.ExecuteNonQueryAsync(cancellationToken);
                _logger.LogInformation("Created sample user: {Name} ({Email})", name, email);
            }

            _logger.LogInformation("Successfully seeded {Count} sample users", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding sample data");
            throw;
        }
    }
}