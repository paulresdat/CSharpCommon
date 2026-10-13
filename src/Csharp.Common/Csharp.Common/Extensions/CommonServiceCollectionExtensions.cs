using Csharp.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Extensions;

public static class CommonServiceCollectionExtensions
{
    public static IServiceCollection AddConsoleOutput(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<IConsoleOutput, ConsoleOutput>();
        return serviceCollection;
    }

    public static IConsoleOutput ConsoleOutput(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<IConsoleOutput>();
    }
}
