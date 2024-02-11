using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Services;

/// <summary>
/// <para>
/// The Service Configuration class allows for one data structure to encapsulate the algorithms necessary to bootstrap
/// an entire application with one line of code in the calling Program.cs app.  Another added benefit is that this allows
/// for libraries to have a central service collection configuration class that can be used in conjunction with another
/// library such that a library can be bootstrapped into an application without registering all its dependencies from scratch.
/// </para>
///
/// <para>
/// This configuration has 3 main concepts:
///
/// <list type="number">
/// <item><description>IOptions Configuration Section</description></item>
/// <item><description>Service Collection Registration</description></item>
/// <item><description>NLog Configuration Section</description></item>
/// </list>
///
/// When bootstrapping an application's dependencies for dependency injection it's important to note that we've
/// identified 3 main sections for doing so outlined above.  The configuration class has these 3 sections explicitly
/// defined and called in a specific order upon registration.
/// </para>
///
/// <para>
/// *NOTE* You can choose to leave NLog blank or setting configurations
/// </para>
/// </summary>
/// <example>
/// <code>
/// public class ServiceConfigurationTest : ServiceConfiguration{ServiceConfigurationTest}, IServiceConfiguration
/// {
///     protected override void AddConfigurations(IServiceCollection serviceCollection)
///     {
///         var configuration = ConfigurationBuilder;
///         // Ioptions section
///         serviceCollection.Configure{MySettings}(configuration.MySettings);
///     }
/// 
///     protected override void ConfigureServiceCollection(IServiceCollection serviceCollection)
///     {
///         serviceCollection.AddSingleton{IMyObject, MyObject}();
///     }
/// 
///     protected override void InjectNlog(IServiceCollection serviceCollection)
///     {
///         serviceCollection.AddLogging(loggingBuilder =>
///         {
///             loggingBuilder.ClearProviders();
///             loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
///             loggingBuilder.AddNLog();
///         });
///     }
/// }
/// </code>
/// </example>
public abstract class ServiceConfiguration
{
    protected IServiceCollectionBuilderConfiguration? ConfigurationBuilder { get; set; }

    protected void Configure(Action<IServiceCollectionBuilderConfiguration?> action)
    {
        action(ConfigurationBuilder);
    }

    protected T GetConfiguration<T>() => (T) (ConfigurationBuilder ?? throw new InvalidOperationException());
    protected T? GetNullableConfiguration<T>() => (T?) ConfigurationBuilder;

    public void SetAppSettingsBuilder(IServiceCollectionBuilderConfiguration? configurationBuilder)
    {
        ConfigurationBuilder = configurationBuilder;
    }

    public void ConfigureServices(IServiceCollection serviceCollection)
    {
        // enforcing here, makes it easier
        serviceCollection.AddLogging();
        serviceCollection.AddOptions();
        AddConfigurations(serviceCollection);
        ConfigureServiceCollection(serviceCollection);
        PostConfiguration(serviceCollection);
        RunBootstrappedAutomapperConfigurations(serviceCollection);
    }
    
    protected abstract void AddConfigurations(IServiceCollection serviceCollection);
    protected abstract void ConfigureServiceCollection(IServiceCollection serviceCollection);
    protected abstract void PostConfiguration(IServiceCollection serviceCollection);

    /// <summary>
    /// Instance level
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="builderConfiguration"></param>
    /// <returns></returns>
    public void Register(IServiceCollection serviceCollection,
        IServiceCollectionBuilderConfiguration builderConfiguration)
    {
        SetAppSettingsBuilder(builderConfiguration);
        ConfigureServices(serviceCollection);
    }

    private List<Action<IMapperConfigurationExpression>> MappingConfigurations { get; set; } = new();

    protected void AddAutoMapper(Action<IMapperConfigurationExpression> profileAction)
    {
        MappingConfigurations.Add(profileAction);
    }

    private void RunBootstrappedAutomapperConfigurations(IServiceCollection serviceCollection)
    {
        var mapper = new MapperConfiguration(cfg =>
        {
            foreach (var config in MappingConfigurations)
            {
                config(cfg);
            }
        });
        serviceCollection.AddSingleton(mapper);
        serviceCollection.AddSingleton(mapper.CreateMapper());
    }
}

public abstract class ServiceConfiguration<TParent> : ServiceConfiguration where TParent : IServiceConfiguration
{
    private static TParent? PrivateInstance { get; set; }
    public static TParent Instance
    {
        get
        {
            if (PrivateInstance != null)
            {
                return PrivateInstance;
            }
            var parent = Activator.CreateInstance<TParent>();
            return parent;
        }
    }

    public static IServiceConfiguration RegisterBaseServices(
        IServiceCollection serviceCollection,
        IServiceCollectionBuilderConfiguration builderConfiguration)
    {
        var instance = Activator.CreateInstance<TParent>();
        PrivateInstance = instance;
        instance.SetAppSettingsBuilder(builderConfiguration);
        instance.ConfigureServices(serviceCollection);
        return PrivateInstance;
    }
}
