using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace Csharp.Common.EntityFramework.UnitTesting.UnitTests;

/// <summary>
/// 
/// </summary>
public abstract class BaseIntegrationTest : 
    EfBaseUnitTest, IDisposable
{
    private bool _inTransaction;
    protected bool InTransaction => _inTransaction;

    private IAppDbContext? _dbContext;
    private IAppDbContext DbContext => _dbContext ?? 
        throw new InvalidOperationException("DB context is null, did you start the transaction?");

    protected bool DbContextIsNull => _dbContext is null;

    private readonly ITestOutputHelper _output;
    protected ITestOutputHelper Output => _output;

    protected void WriteLine(params object[] data)
    {
        foreach (var d in data)
        {
            _output.WriteLine(d.ToString());
        }
    }

    protected BaseIntegrationTest(ITestOutputHelper output)
    {
        _output = output;
    }

    protected void SetDbContext(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// This starts the test transaction and implicitly creates the service provider for you.
    /// </summary>
    protected virtual void StartTestTransaction()
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

    protected T GetDbContext<T>() where T: IAppDbContext
    {
        if (!InTransaction)
        {
            throw new InvalidOperationException(
                "To use the DbContext factory, you must bootstrap your dependencies and then call " +
                "StartTestTransaction() at the end of your constructor.  Please read the documentation " +
                "on the base integration test framework");
        }
        return (T) DbContext;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="T"></typeparam>
    protected virtual void MockWithDb<T, T2>(Action<Mock<T>, T2, IServiceProvider> func) where T : class where T2 : class, IAppDbContext
    {
        // this can be dangerous outside of a transaction
        var temporaryServiceProvider = ServiceCollection.BuildServiceProvider();
        var dbContext = temporaryServiceProvider.GetRequiredService<T2>();
        var inst = (Mock<T>?) Activator.CreateInstance(typeof(Mock<T>));
        func(inst!, dbContext, temporaryServiceProvider);
        ServiceCollection.RefreshSingleton(inst!.Object);
    }
}
