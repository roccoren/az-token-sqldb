using System.Data;

namespace AzureSqlTokenAuth.Data;

public interface IConnectionManager : IDisposable
{
    Task<IDbConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}