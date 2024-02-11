using Csharp.Common.AppSettings;
using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace Csharp.Common.EntityFramework.UnitTesting.UnitTests;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public abstract class DbIntegrationTest<TDbContext> : BaseIntegrationTest
    where TDbContext : class, IAppDbContext
{
    protected DbIntegrationTest(ITestOutputHelper output) : base(output)
    {
    }

    protected override void StartTestTransaction()
    {
        if (DbContextIsNull)
        {
            ServiceCollection.AddSingleton<TDbContext>();
            SetDbContext(ServiceProvider.GetRequiredService<TDbContext>());
        }
        base.StartTestTransaction();
    }

    /// <summary>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    protected TDbContext DbContext => GetDbContext<TDbContext>();

    protected override void MockWithDb<T, T2>(Action<Mock<T>, T2, IServiceProvider> func)
    {
        ServiceCollection.AddSingleton<TDbContext>();
        base.MockWithDb(func);
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TIDbContext"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
public abstract class DbIntegrationTest<TIDbContext, TDbContext> : DbIntegrationTest<TDbContext>
    where TIDbContext : class, IAppDbContext
    where TDbContext : DbContext, TIDbContext
{
    protected DbIntegrationTest(ITestOutputHelper output) : base(output)
    {
        
    }

    protected new void StartTestTransaction()
    {
        if (DbContextIsNull)
        {
            ServiceCollection.AddSingleton<TIDbContext, TDbContext>();
            SetDbContext(ServiceProvider.GetRequiredService<TIDbContext>());
        }
        base.StartTestTransaction();
    }

    protected new TIDbContext DbContext => GetDbContext<TIDbContext>();
    
    protected override void MockWithDb<T, T2>(Action<Mock<T>, T2, IServiceProvider> func)
    {
        ServiceCollection.AddSingleton<TIDbContext, TDbContext>();
        base.MockWithDb(func);
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TIDbContext"></typeparam>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TConfigurationContext"></typeparam>
public abstract class DbIntegrationTest<TIDbContext, TDbContext, TConfigurationContext> 
    : DbIntegrationTest<TIDbContext, TDbContext>, IIntegrationTestConfiguration<TConfigurationContext>
    where TIDbContext : class, IAppDbContext
    where TDbContext : DbContext, TIDbContext
    where TConfigurationContext : IServiceCollectionBuilderConfiguration
{
    protected DbIntegrationTest(ITestOutputHelper output) : base(output)
    {
        
    }

    private TConfigurationContext? _configuration;
    private TConfigurationContext Configuration
    {
        set => _configuration = value;
        get => _configuration ?? throw new InvalidOperationException();
    }
    
    protected IIntegrationTestConfiguration<TConfigurationContext> SetupConfiguration()
    {
        ServiceCollection.AddLogging();
        ServiceCollection.AddOptions();
        // some configurations inject the ConfigurationBuilder
        if (typeof(TConfigurationContext).GetConstructors()[0].GetParameters().Length == 1)
        {
            var configuration = new ConfigurationBuilder();
            Configuration =
                (TConfigurationContext)Activator.CreateInstance(typeof(TConfigurationContext), args: new object[] { configuration })! 
                ?? throw new InvalidOperationException();
        }
        else
        {
            Configuration =
                (TConfigurationContext)Activator.CreateInstance(typeof(TConfigurationContext))! 
                ?? throw new InvalidOperationException();
        }
        return this;
    }
    
    public IIntegrationTestConfiguration<TConfigurationContext> Configure<TConfigurationClass>(Func<TConfigurationContext, IConfigurationSection> expr)
        where TConfigurationClass : class
    {
        var configurationSection = expr.Invoke(Configuration);
        ServiceCollection.Configure<TConfigurationClass>(configurationSection);
        return this;
    }
    
    public IIntegrationTestConfiguration<TConfigurationContext> MockConfigure<TConfigurationClass>(
        TConfigurationClass optionValue)
        where TConfigurationClass : class
    {
        MockOption<TConfigurationClass>(x => x.SetupGet(y => y.Value).Returns(optionValue));
        return this;
    }
    
    public IIntegrationTestConfiguration<TConfigurationContext> MockWatcher<TWatcher, TReturnObject>(
        TReturnObject optionValue)
        where TReturnObject : class, new()
        where TWatcher : class, IAppSettingsWatcher<TReturnObject>
    {
        Mock<TWatcher>(m =>
        {
            m.SetupGet(opt => opt.Settings).Returns(optionValue);
        });
    
        return this;
    }
}

public interface IIntegrationTestConfiguration<TConfigurationContext> where TConfigurationContext : IServiceCollectionBuilderConfiguration
{
    public IIntegrationTestConfiguration<TConfigurationContext> Configure<TConfigurationClass>(
        Func<TConfigurationContext, IConfigurationSection> expr)
        where TConfigurationClass : class;

    public IIntegrationTestConfiguration<TConfigurationContext> MockConfigure<TConfigurationClass>(
        TConfigurationClass optionValue)
        where TConfigurationClass : class;

    public IIntegrationTestConfiguration<TConfigurationContext> MockWatcher<TWatcher, TReturnObject>(
        TReturnObject optionValue)
        where TReturnObject : class, new()
        where TWatcher : class, IAppSettingsWatcher<TReturnObject>;
}
