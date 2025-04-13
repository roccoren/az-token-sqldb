namespace AzureSqlTokenAuth.Data;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
    Task SeedSampleDataAsync(int count = 10, CancellationToken cancellationToken = default);
}