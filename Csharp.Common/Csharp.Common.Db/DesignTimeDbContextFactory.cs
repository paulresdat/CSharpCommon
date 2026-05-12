using Csharp.Common.EntityFramework.Domain;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Db;

// public class DesignTimeDbContextFactory : DesignTimeDbContextFactoryBase<CsharpCommonTestingDbContext>
// {
//     protected override void ConfigureBuilder(ConfigurationBuilder builder)
//     {
//         builder
//             .SetBasePath(Directory.GetCurrentDirectory())
//             .AddJsonFile("appsettings.db-test.json", optional: false, reloadOnChange: true);
//     }
//
//     protected override void DbContextSetup(ServiceCollection serviceCollection, IConfigurationRoot configurationRoot)
//     {
//         serviceCollection.AddOptions();
//         serviceCollection.Configure<DbOptions>(configurationRoot.GetSection("DbContextOptions"));
//     }
// }

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CsharpCommonTestingDbContext>
{
    public CsharpCommonTestingDbContext CreateDbContext(string[] args)
    {
        var configuration = GetConfiguration();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions();
        serviceCollection.Configure<DbOptions>(configuration.GetSection("DbContextOptions"));
        serviceCollection.AddDbContext<CsharpCommonTestingDbContext>();

        var provider = serviceCollection.BuildServiceProvider();
        return provider.GetRequiredService<CsharpCommonTestingDbContext>();
    }

    public static IConfigurationRoot GetConfiguration()
    {
        // bring in configuration manager
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.db-test.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<CsharpCommonTestingDbContext>();

        var configuration = builder.Build();
        Console.WriteLine("Running migrations on: " + configuration.GetValue<string>("DbContextOptions:ConnectionStrings:DbContext"));
        // var serviceCollection = new ServiceCollection();
        // serviceCollection.AddOptions();
        // serviceCollection.Configure<RtkRoutingDbContextOptions>(configuration.GetSection("DbContextOptions"));
        // var options = serviceCollection.BuildServiceProvider()
        //     .GetRequiredService<IOptions<RtkRoutingDbContextOptions>>();
        // Console.WriteLine("Option value: " + options.Value.ConnectionStrings.DbContext);
        return configuration;
    }
}