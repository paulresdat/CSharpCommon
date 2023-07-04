using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.EntityFramework.Domain;

/// <summary>
/// The design time db context factory is what EF Core uses to get the configuration context for running/creating
/// migrations in code first projects.  This abstract class standardizes the approach of using it within many
/// .NET framework environments that uses code first, from Blazor to Console apps.
/// 
/// Using this pattern can be useful as central design around configuring and using contexts such that fetching
/// integration testing configurations that are the same as what you would fetch when running migrations, can
/// prove to be very useful when taking into consideration user secrets on projects that have more than one
/// person working on it.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public abstract class DesignTimeDbContextFactoryBase<TDbContext> : IDesignTimeDbContextFactory<TDbContext> 
    where TDbContext : DbContext, IAppDbContext
{
    public TDbContext CreateDbContext(string[] args)
    {
        var configuration = GetConfiguration<TDbContext>();
        var serviceCollection = new ServiceCollection();
        DbContextSetup(serviceCollection, configuration);
        serviceCollection.AddDbContext<TDbContext>();

        var provider = serviceCollection.BuildServiceProvider();
        return provider.GetRequiredService<TDbContext>();
    }

    /// <summary>
    /// <para>
    /// This is a required implementation for configuring the builder.
    /// <example>
    /// <code>
    /// protected override void ConfigureBuilder(ConfigurationBuilder builder)
    /// {
    ///     builder.SetBasePath(Directory.GetCurrentDirectory())
    ///         .AddJsonFile(FileName + ".json", optional: false, reloadOnChange: true);
    /// }
    /// </code>
    /// </example>
    /// </para>
    /// </summary>
    /// <param name="builder"></param>
    protected abstract void ConfigureBuilder(ConfigurationBuilder builder);

    /// <summary>
    /// <para>
    /// After configuring the configuration builder (ConfigureBuilder), this is called to setup the data context.
    /// </para>
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="configurationRoot"></param>
    protected abstract void DbContextSetup(ServiceCollection serviceCollection, IConfigurationRoot configurationRoot);

    public IConfigurationRoot GetConfiguration<TAssembly>()
    {
        var builder = new ConfigurationBuilder();
        ConfigureBuilder(builder);
        builder.AddUserSecrets(typeof(TAssembly).GetTypeInfo().Assembly);
        return builder.Build();
    }
}