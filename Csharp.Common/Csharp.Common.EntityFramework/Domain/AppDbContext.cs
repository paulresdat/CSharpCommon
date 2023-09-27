using Csharp.Common.EntityFramework.Domain.Options;
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

/// <summary>
/// 
/// </summary>
public interface IAppDbContextTransaction : IAppDbContext
{
    void StartTransaction();
    Task StartTransactionAsync();
    void RollbackTransaction();
    Task RollbackTransactionAsync();
    void CommitTransaction();
    Task CommitTransactionAsync();

    void SavePoint(string savePoint);
    Task SavePointAsync(string savePoint);
    void RollbackToSavePoint(string savePoint);
    Task RollbackToSavePointAsync(string savePoint);
}

/// <summary>
/// 
/// </summary>
public abstract class AppDbContext : DbContext, IAppDbContextTransaction, IAppDbContextTesting
{
    protected readonly string ConnectionString;
    private IDbContextTransaction? Transaction { get; set; }

    protected AppDbContext(IOptions<IAppDbContextOptions> options)
    {
        var dbContextOptions = options.Value;
        ConnectionString = dbContextOptions.ConnectionStrings?.DbContext ?? "";
    }

    /// <summary>
    /// </summary>
    public void StartTestTransaction()
    {
        Transaction = Database.BeginTransaction();
    }

    /// <summary>
    /// </summary>
    public void EndTestTransaction()
    {
        Transaction?.Rollback();
    }

    /// <summary>
    /// </summary>
    public void StartTransaction()
    {
        if (Transaction is not null)
        {
            throw new InvalidOperationException("Can not call transaction more than once");
        }
        Transaction = Database.BeginTransaction();
    }

    /// <summary>
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task StartTransactionAsync()
    {
        if (Transaction is not null)
        {
            throw new InvalidOperationException("Can not call transaction more than once");
        }

        Transaction = await Database.BeginTransactionAsync();
    }

    /// <summary>
    /// </summary>
    public void RollbackTransaction()
    {
        if (Transaction is not null)
        {
            Transaction.Rollback();
        }
    }

    /// <summary>
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (Transaction is not null)
        {
            await Transaction.RollbackAsync();
        }
    }

    /// <summary>
    /// </summary>
    public void CommitTransaction()
    {
        if (Transaction is not null)
        {
            Transaction.Commit();
        }
    }

    /// <summary>
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        
        if (Transaction is not null)
        {
            await Transaction.CommitAsync();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="savePoint"></param>
    public void SavePoint(string savePoint)
    {
        if (Transaction is not null && Transaction.SupportsSavepoints)
        {
            Transaction.CreateSavepoint(savePoint);
        }
        CheckForInvalidSavePointCall();
    }

    /// <summary>
    /// </summary>
    /// <param name="savePoint"></param>
    public async Task SavePointAsync(string savePoint)
    {
        if (Transaction is not null && Transaction.SupportsSavepoints)
        {
            await Transaction.CreateSavepointAsync(savePoint);
        }
        CheckForInvalidSavePointCall();
    }

    /// <summary>
    /// </summary>
    /// <param name="savePoint"></param>
    public void RollbackToSavePoint(string savePoint)
    {
        if (Transaction is not null && Transaction.SupportsSavepoints)
        {
            Transaction.RollbackToSavepoint(savePoint);
        }
        CheckForInvalidSavePointCall();
    }

    /// <summary>
    /// </summary>
    /// <param name="savePoint"></param>
    public async Task RollbackToSavePointAsync(string savePoint)
    {
        if (Transaction is not null && Transaction.SupportsSavepoints)
        {
            await Transaction.RollbackToSavepointAsync(savePoint);
        }
        CheckForInvalidSavePointCall();
    }

    private void CheckForInvalidSavePointCall()
    {
        if (Transaction is not null && !Transaction.SupportsSavepoints)
        {
            throw new InvalidOperationException("Save points are not supported");
        }
    }
}