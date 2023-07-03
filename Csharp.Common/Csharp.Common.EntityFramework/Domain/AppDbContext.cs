using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace Csharp.Common.EntityFramework.Domain;

/// <summary>
/// IAppDbContext allows for a slim down version of what can can be exposed from the DbContext
/// class.  By design, it's simple as possible. As you require more properties of the DbContext
/// to be exposed, you can indeed continue to extend the interface into your own, specific
/// for that project.  If there's a pressing need to expose it early on (which can definitely
/// be the case), we can update this interface.
/// </summary>
public interface IAppDbContext
{
    int SaveChanges();
    DatabaseFacade Database { get; }
    EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
}

public interface IAppDbContextTesting
{
    void StartTestTransaction();
    void EndTestTransaction();
}

public abstract class AppDbContext : DbContext, IAppDbContext, IAppDbContextTesting
{
    protected readonly string ConnectionString;
    private IDbContextTransaction? Transaction { get; set; }

    protected AppDbContext(IOptions<IAppDbContextOptions> options)
    {
        var dbContextOptions = options.Value;
        ConnectionString = dbContextOptions.ConnectionStrings?.DbContext ?? "";
    }

    public void StartTestTransaction()
    {
        Transaction = Database.BeginTransaction();
    }

    public void EndTestTransaction()
    {
        Database.RollbackTransaction();
    }
}