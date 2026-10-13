using System.Transactions;
using Csharp.Common.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Csharp.Common.EntityFramework.UnitTesting.UnitTests;

/// <summary>
/// This class provides the basic boiler for creating a test transaction from a global TransactionScope.  This transaction
/// is injected as the transaction to use for every instead of DbContext that's generated from the service scope pattern
/// in the Repository Pattern provided from Csharp.Common.EntityFramework and works out of the box with AddScoped
/// and AddDbContext structuring.
/// </summary>
public abstract class IntegrationTestingFixture : BaseSingleServiceProviderUnitTesting, IAsyncLifetime, IDisposable
{
    private IServiceScope? _scope;
    private TransactionScope? _tx;
    protected override IServiceProvider ServiceProvider => _scope?.ServiceProvider
        ?? throw new InvalidOperationException("Start Test Transaction must be called");

    protected void StartTestTransaction()
    {
        _scope = Services.BuildServiceProvider().CreateScope();
    }

    protected void AddDbContext<TDbContext>() where TDbContext : DbContext
    {
        Services.AddDbContext<TDbContext>();
    }
    protected void AddDbContext<TIDbContext, TDbContext>() where TIDbContext : class where TDbContext : DbContext, TIDbContext
    {
        Services.AddDbContext<TIDbContext, TDbContext>();
    }

    public Task InitializeAsync()
    {
        _tx = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { },
            TransactionScopeAsyncFlowOption.Enabled);
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _tx?.Dispose();
        _tx = null;
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}