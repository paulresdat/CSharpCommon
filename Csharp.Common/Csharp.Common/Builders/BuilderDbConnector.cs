using Microsoft.Extensions.DependencyInjection;

namespace Csharp.Common.Builders;

/// <summary>
/// <para>This db connect class handles the population of the DbContext object.</para>
///
/// <para>
/// You do not need to extend from this class directly.
/// Please extend the <see cref="BuilderDbContext{TModel,TDbContext,TParentBuilder}">BuilderDbContext</see>
/// object instead.
/// </para>
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TParentModel"></typeparam>
public abstract class BuilderDbConnector<TDbContext, TModel, TParentModel> : BuilderAbstract<TModel, TParentModel> 
    where TDbContext: class 
    where TModel: class, new()
    where TParentModel: BuilderAbstract<TModel, TParentModel>, new()
{
    private IServiceScopeFactory? _serviceScopeFactory;

    protected void RegisterBuilderDbConnectorServiceScopeFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected void RegisterTheDbContextExplicitlyRatherThanScopeFactory(TDbContext dbContext)
    {
        PrivateContext = dbContext;
    }

    private TDbContext? PrivateContext { get; set; }

    protected TDbContext DbContext
    {
        get
        {
            if (PrivateContext == null)
            {
                var scope = _serviceScopeFactory?.CreateScope();
                PrivateContext = scope?.ServiceProvider.GetRequiredService<TDbContext>()
                    ?? throw new BuilderException("Invalid db context provided for database builder");
            }
            
            return PrivateContext;
        }
    }
}

public class BuilderException : Exception
{
    public BuilderException(string message) : base(message)
    {
    }
}
