using Csharp.Common.Utilities.ServiceCollection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.EntityFramework.Utilities.ServiceCollection;

public interface IEfCoreServiceConfiguration : IServiceConfiguration
{
    /// <summary>
    /// TODO - add documentation (see concrete)
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="addAsSingleton"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TConcrete"></typeparam>
    /// <returns></returns>
    IServiceConfiguration InjectDbContext<TInterface, TConcrete>(IServiceCollection serviceCollection,
        bool addAsSingleton = false)
        where TConcrete : DbContext, TInterface
        where TInterface : class;

    IServiceConfiguration InjectDbContext<TConcrete>(IServiceCollection serviceCollection,
        bool addAsSingleton = false)
        where TConcrete : DbContext;

    /// <summary>
    /// <para>Identity framework and user store work around, WEIRD BUT IT IS WHAT IT IS.</para>
    /// 
    /// <para>
    /// In most cases we want to register a singleton or AddDbContext for service scoping, but in the case of
    /// Identity, it wants a concrete and not the interface.  The InjectDbContextForUserStore ensures that the concrete
    /// is provided, yet we can still ask for the db context with its interface for our repositories.  We can also use
    /// the singleton pattern like the others for our integration tests.
    /// </para>
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="useSingleton"></param>
    /// <typeparam name="TInterface"></typeparam>
    /// <typeparam name="TConcrete"></typeparam>
    /// <returns></returns>
    IServiceConfiguration InjectDbContextForUserStore<TInterface, TConcrete>(
        IServiceCollection serviceCollection, bool useSingleton = false)
        where TConcrete : DbContext, TInterface
        where TInterface : class;
}