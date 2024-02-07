using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Builders;

/// <summary>
/// TODO - requires documentation and example code
/// </summary>
public class BuilderFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public BuilderFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public T Fetch<T>() where T : class, new()
    {
        if (typeof(T).GetInterfaces().Contains(typeof(IBuilderServiceScope)))
        {
            var builder = new T();
            ((IBuilderServiceScope) builder).RegisterServiceScope(_serviceScopeFactory);
            return builder;
        }

        if (typeof(T).GetInterfaces().Contains(typeof(IBuilder)))
        {
            return new T();
        }

        throw new BuilderFactoryException(
            "Unknown assignable type provided for BuilderFactory: " + typeof(T).FullName);
    }
}

public class BuilderFactoryException : Exception
{
    public BuilderFactoryException(string message) : base(message)
    {
        
    }
}
