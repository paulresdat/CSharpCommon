using System.Data.SqlClient;
using AutoMapper;
using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.EntityFramework.Domain.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Csharp.Common.EntityFramework.Repositories;

public abstract class RepositoryBase<TParentRepository, TDbContext>
    where TParentRepository : class // Specific Repository
    where TDbContext: class, IAppDbContext // Specific DbContext
{
    protected readonly MapperConfiguration Mapper;
    protected readonly ILogger<TParentRepository> Logger;
    protected readonly IOptions<IAppDbContextOptions> DbContextOptions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public event TransmitRepositoryError? TransmitErrorMessage;

    protected RepositoryBase(
        MapperConfiguration config,
        // ReSharper disable once ContextualLoggerProblem
        ILogger<TParentRepository> logger,
        IOptions<IAppDbContextOptions> dbContextOptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        Mapper = config;
        Logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        DbContextOptions = dbContextOptions;
    }

    #region transaction region
    /// <summary>
    /// </summary>
    /// <param name="queryToRun"></param>
    /// <param name="methodName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected async Task<T> RunTransactionAsync<T>(Func<IAppDbContextTransaction, Task<T>> queryToRun,
        string methodName = nameof(RunTransactionAsync))
    {
        return await RunTransaction(queryToRun, methodName);
    }

    /// <summary>
    /// </summary>
    /// <param name="queryToRun"></param>
    /// <param name="methodName"></param>
    protected async Task RunTransactionAsync(Func<IAppDbContextTransaction, Task> queryToRun,
        string methodName = nameof(RunTransactionAsync))
    {
        await RunTransaction(queryToRun, methodName);
    }

    /// <summary>
    /// </summary>
    /// <param name="queryToRun"></param>
    /// <param name="methodName"></param>
    protected void RunTransaction(Action<IAppDbContextTransaction> queryToRun, string methodName = nameof(RunTransaction))
    {
        RunQuery(dbContext =>
        {
            var transactionalDbContext = (IAppDbContextTransaction)dbContext;
            transactionalDbContext.StartTransaction();
            try
            {
                queryToRun(transactionalDbContext);
                transactionalDbContext.CommitTransaction();
            }
            catch (Exception)
            {
                transactionalDbContext.RollbackTransaction();
                throw;
            }
        }, methodName);
    }

    /// <summary>
    /// </summary>
    /// <param name="queryToRun"></param>
    /// <param name="methodName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T RunTransaction<T>(Func<IAppDbContextTransaction, T> queryToRun, string methodName = nameof(RunTransaction))
    {
        return RunQuery(dbContext =>
        {
            var transactionalDbContext = (IAppDbContextTransaction)dbContext;
            transactionalDbContext.StartTransaction();
            try
            {
                var val = queryToRun(transactionalDbContext);
                transactionalDbContext.CommitTransaction();
                return val;
            }
            catch (Exception)
            {
                transactionalDbContext.RollbackTransaction();
                throw;
            }
        }, methodName);
    }
    #endregion

    #region run query
    /// <summary>
    /// </summary>
    /// <param name="queryToRun"></param>
    /// <param name="methodName"></param>
    protected async Task RunQueryAsync(Func<TDbContext, Task> queryToRun, string methodName = nameof(RunQueryAsync))
    {
        // this may require more code to correctly, but for now this is good enough
        await RunQuery(queryToRun, methodName);
    }

    /// <summary>
    /// </summary>
    /// <param name="queryToRun"></param>
    /// <param name="methodName"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected async Task<T> RunQueryAsync<T>(Func<TDbContext, Task<T>> queryToRun,
        string methodName = nameof(RunQueryAsync))
    {
        return await RunQuery(queryToRun, methodName);
    }

    protected void RunQuery(Action<TDbContext> queryToRun, string methodName = nameof(RunQuery))
    {
        RunQuery(dbContext =>
        {
            queryToRun(dbContext);
            return 1;
        }, methodName);
    }
    protected T RunQuery<T>(Func<TDbContext, T> queryToRun, string methodName = nameof(RunQuery))
    {
        try
        {
            Logger.LogTrace("Running query for: {MethodName}", methodName);
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                return queryToRun(dbContext);
            }
        }
        catch (Exception exc) when (exc.InnerException is SqlException || exc is SqlException)
        {
            Logger.LogError(exc, "An SqlException occurred, will retry running the query");
            Logger.LogDebug("Retry query: {ExceptionsEncountered}", ExceptionsEncountered);
            var totalExceptions = new List<Exception>
            {
                exc,
            };

            // we likely have a resiliency issue with this particular type of exception for deadlocks, all other exceptions we will
            // allow to float to the top for continued hardening and error handling
            // Uses the exponential back off algorithm
            for (var i = 0; i < DbContextOptions.Value.NumberOfQueryRetriesBeforeSendingException; i++)
            {
                ExceptionsEncountered++;
                // the delay starts off in the milliseconds 10's of milliseconds and doesn't grow into hundreds of milliseconds
                // until around the 5th retry
                var millisecondDelay = GetNextDelay();
                Thread.Sleep(millisecondDelay);

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
                        return queryToRun(dbContext);
                    }
                }
                catch (Exception ex)
                {
                    totalExceptions.Add(ex);
                    Logger.LogError(ex, "An Exception occurred while performing a retry on the current query");
                    Logger.LogDebug("Retry query: {ExceptionsEncountered}", ExceptionsEncountered);
                }
            }

            Logger.LogCritical(
                "Max retry reached: {NumberOfQueryRetriesBeforeSendingException}.  Floating exception back up to the top",
                DbContextOptions.Value.NumberOfQueryRetriesBeforeSendingException);
            // When max amount of retries happen, there might be a bigger issue so throw an exception
            throw new RepositoryBaseException("Max retry reached: " + DbContextOptions.Value.NumberOfQueryRetriesBeforeSendingException, totalExceptions);
        }
        catch (Exception ex)
        {
            // log the exception
            Logger.LogCritical(ex, "An exception was thrown running a repository query: {MethodName}", methodName);
            SendErrorMessage($"An exception was thrown in {methodName}", ex);
            // throw the exception up the chain
            // the exception needs to be handled in the infrastructure layer
            throw;
        }
    }
    #endregion

    #region Get Next Delay
    private int ExceptionsEncountered { get; set; }
    private Random Random { get; } = new Random();
    private int RandomDelayMaxValue { get; set; } = 200;
    private int MaxDelay { get; set; } = 10000;
    /// <summary>
    /// Use the ExceptionEncountered counter to determine
    /// </summary>
    /// <returns></returns>
    private TimeSpan GetNextDelay()
    {
        // Exponential Back Off Algorithm
        var delta = Math.Min(Math.Pow(2, ExceptionsEncountered) + Random.Next(1, RandomDelayMaxValue), MaxDelay);
        return TimeSpan.FromMilliseconds(delta);
    }
    #endregion

    private void SendErrorMessage(string message, Exception ex)
    {
        TransmitErrorMessage?.Invoke(new RepositoryErrorMessage {
            Error = RepositoryError.SqlClientError,
            Message = message,
            Exception = ex,
        });
    }
}