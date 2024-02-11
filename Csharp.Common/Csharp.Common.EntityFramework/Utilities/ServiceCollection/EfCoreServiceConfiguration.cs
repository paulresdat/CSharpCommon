using Csharp.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.EntityFramework.Utilities.ServiceCollection;

public abstract class EfCoreServiceConfiguration<TParent> : 
    ServiceConfiguration<TParent> where TParent : IServiceConfiguration
{
    public static IEfCoreServiceConfiguration RegisterServices(
        IServiceCollection serviceCollection,
        IServiceCollectionBuilderConfiguration builderConfiguration)
    {
        return (IEfCoreServiceConfiguration) RegisterBaseServices(serviceCollection, builderConfiguration);
    }

    public IServiceConfiguration InjectDbContextForUserStore<TInterface, TConcrete>(
        IServiceCollection serviceCollection, bool useSingleton = false)
        where TConcrete : DbContext, TInterface
        where TInterface : class
    {
        if (!useSingleton)
        {
            serviceCollection.AddDbContext<TConcrete>();
            serviceCollection.AddScoped(s => (TInterface) s.GetRequiredService<TConcrete>());
        }
        else
        {
            serviceCollection.AddSingleton<TConcrete>();
            serviceCollection.AddSingleton(s => (TInterface) s.GetRequiredService<TConcrete>());
        }
        return (IServiceConfiguration) this;
    }

    public IServiceConfiguration InjectDbContext<TInterface, TConcrete>(IServiceCollection serviceCollection, bool addAsSingleton = false)
        where TConcrete : DbContext, TInterface
        where TInterface : class
    {
        if (addAsSingleton)
        {
            serviceCollection.AddSingleton<TInterface, TConcrete>();
        }
        else
        {
            serviceCollection.AddDbContext<TInterface, TConcrete>();
        }
        return (IServiceConfiguration)this;
    }

    public IServiceConfiguration InjectDbContext<TConcrete>(IServiceCollection serviceCollection, bool addAsSingleton = false)
        where TConcrete : DbContext
    {
        if (addAsSingleton)
        {
            serviceCollection.AddSingleton<TConcrete>();
        }
        else
        {
            serviceCollection.AddDbContext<TConcrete>();
        }
        return (IServiceConfiguration)this;
    }
}