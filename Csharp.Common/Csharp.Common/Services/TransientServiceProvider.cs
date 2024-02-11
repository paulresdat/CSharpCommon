using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Services;

// use this for your transients
public interface ITransientService { }

public interface ITransientServiceProvider
{
    T? GetTransient<T>() where T : ITransientService;
    T GetRequiredTransient<T>() where T : ITransientService;
}

/// <summary>
/// <para>
/// This class acts as a wrapper for fetching transients within your singleton context.  This takes away the need
/// for injecting the IServiceScopeFactory all around your app too.  Also, it does enforce thinking about what it is
/// you're really trying to fetch in your application.  ServiceProvider should never be provided within *ANY* of your
/// classes, ever.  And if you find yourself thinking you need to, you need to start thinking WHY.  If you think you need
/// to inject the ServiceProvider, then you'll want the ServiceScopeFactory instead.  However, instead of using the
/// ServiceScopeFactory everywhere, you should think about using this because it's explicit, limits scope resolution
/// everywhere and is a clean code approach.  DON'T INTRODUCE ANTI-PATTERNS.
/// </para>
///
/// <example>
/// This example demonstrates use:
/// <code>
/// public class MyTransient : ITransientService
/// {
///   ... properties ...
/// }
/// 
/// // register the singleton
/// ServiceCollection.AddSingleton&lt;ITransientServiceProvider, TransientServiceProvider&gt;();
///
/// // register the transient
/// ServiceCollection.AddTransient&lt;MyTransient&gt;();
///
/// // in your service inject the transient service provider and use it like so
/// var myTransient = _transientServiceProvider.GetRequiredTransient&lt;MyTransient&gt;();
/// </code>
/// </example>
/// </summary>
public class TransientServiceProvider : ITransientServiceProvider
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public TransientServiceProvider(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public T? GetTransient<T>() where T : ITransientService
    {
        var scopedServiceProvider = _serviceScopeFactory.CreateScope();
        return scopedServiceProvider.ServiceProvider.GetService<T>();
    }

    public T GetRequiredTransient<T>() where T : ITransientService
    {
        // do NOT use the using statement in case the transient fetched has the IDisposable interface
        var scopedServiceProvider = _serviceScopeFactory.CreateScope();
        return scopedServiceProvider.ServiceProvider.GetRequiredService<T>();
    }
}
