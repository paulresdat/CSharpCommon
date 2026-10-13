using Csharp.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Csharp.Common.UnitTesting.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Refreshes a singleton with a mocked object wrapper see
    /// <see cref="RefreshSingleton{T}(Microsoft.Extensions.DependencyInjection.IServiceCollection)">RefreshSingleton{T}(T service)</see>
    /// </summary>
    /// <param name="serviceCollection">service collection to operate on</param>
    /// <param name="addMocked">the mocked object that will be registered</param>
    /// <typeparam name="T">the type that is mocked</typeparam>
    public static void RefreshSingleton<T>(this IServiceCollection serviceCollection, Mock<T> addMocked) where T: class
    {
        // first remove the service
        serviceCollection.Remove<T>();
        serviceCollection.AddSingleton(addMocked.Object);
    }
}