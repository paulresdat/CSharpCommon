using Csharp.Common.EntityFramework.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.EntityFramework.UnitTesting.UnitTests;

/// <summary>
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public abstract class IntegrationTestBase<TDbContext> : EfBaseUnitTest, IDisposable
    where TDbContext : AppDbContext
{
    private TDbContext? _dbContext;
    private bool _inTransaction;

    /// <summary>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    protected TDbContext DbContext
    {
        get
        {
            if (!_inTransaction)
            {
                throw new InvalidOperationException(
                    "To use the DbContext factory, you must bootstrap your dependencies and then call " +
                    "StartTestTransaction() at the end of your constructor.  Please read the documentation " +
                    "on the base integration test framework");
            }
            if (_dbContext is null)
            {
                _dbContext = ServiceProvider.GetRequiredService<TDbContext>();
            }

            return _dbContext;
        }
    }

    /// <summary>
    /// This starts the test transaction and implicitly creates the service provider for you.
    /// </summary>
    protected void StartTestTransaction()
    {
        _inTransaction = true;
        ((IAppDbContextTesting)DbContext).StartTestTransaction();
    }

    /// <summary>
    /// </summary>
    public void Dispose()
    {
        ((IAppDbContextTesting)DbContext).EndTestTransaction();
    }
}