using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Extensions;

/// <summary>
/// This suite of extensions offer helpers around IServiceCollection as extension methods on the interface itself.
/// These are helper functions around removing and refreshing objects which offer better control over the services
/// in your service collection within a unit testing context.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Remove the service from the service collection
    /// </summary>
    /// <param name="services">service collection to operate on</param>
    /// <typeparam name="T">the service type to remove</typeparam>
    /// <returns></returns>
    public static IServiceCollection Remove<T>(this IServiceCollection services)
    {
        var serviceDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(T));
        if (serviceDescriptor != null)
        {
            services.Remove(serviceDescriptor);
        }

        return services;
    }

    /// <summary>
    /// Ensures that a singleton that may have been previously registered will in fact be removed before
    /// adding the singleton back.
    /// </summary>
    /// <param name="serviceCollection">service collection to operate on</param>
    /// <typeparam name="T">the service type to refresh</typeparam>
    public static void RefreshSingleton<T>(this IServiceCollection serviceCollection) where T : class
    {
        serviceCollection.Remove<T>();
        serviceCollection.AddSingleton<T>();
    }

    /// <summary>
    /// Ensures that a singleton that may have previously been added is updated with an already instantiated object.
    /// It will first remove the service it exists in the service collection and then add the new object.  Excellent
    /// for better control over your unit tests.
    /// </summary>
    /// <param name="serviceCollection">service collection to operate on</param>
    /// <param name="service">the concrete class of the service that will be registered</param>
    /// <typeparam name="T">the type of concrete</typeparam>
    public static void RefreshSingleton<T>(this IServiceCollection serviceCollection, T service) where T : class
    {
        serviceCollection.Remove<T>();
        serviceCollection.AddSingleton(service);
    }

    public static void RefreshTransient<T>(this IServiceCollection serviceCollection, T service) where T : class
    {
        serviceCollection.Remove<T>();
        serviceCollection.AddTransient<T>();
    }

    /// <summary>
    /// Refreshes a singleton with a registered interface.
    /// See <see cref="RefreshSingleton{T}(Microsoft.Extensions.DependencyInjection.IServiceCollection)">RefreshSingleton{T}(T service)</see>
    /// </summary>
    /// <param name="serviceCollection">service collection to operate on</param>
    /// <typeparam name="TFrom">the interface of the type</typeparam>
    /// <typeparam name="TTo">the concrete of the type</typeparam>
    public static void RefreshSingleton<TFrom, TTo>(this IServiceCollection serviceCollection) where TFrom : class where TTo : class, TFrom
    {
        serviceCollection.Remove<TFrom>();
        serviceCollection.AddSingleton<TFrom, TTo>();
    }
}
