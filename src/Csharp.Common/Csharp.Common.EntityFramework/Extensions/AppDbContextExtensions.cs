using System.Linq.Expressions;
using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.EntityFramework.Domain.Sql.Procedures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Csharp.Common.EntityFramework.Extensions;

public static class AppDbContextExtensions
{
  private static IReadOnlyList<ITableResetStrategy> Strategies { get; } =
  [
    new SqliteTableResetStrategy(),
    new SqlServerTableResetStrategy(),
  ];

  public static IEntityProcedure CallProcedure(
    this IAppDbContext dbContext,
    string procedureName,
    IEntityProcedure? passedProc = null
  )
  {
    var entityProcedure = passedProc ?? new EntityProcedure();
    entityProcedure.SetDbContext(dbContext).SetProcedure(procedureName);
    return entityProcedure;
  }

  public static IEntityProcedure<T> CallProcedure<T>(
    this IAppDbContext dbContext,
    string procedureName,
    IEntityProcedure<T>? passedProc = null
  )
    where T : class
  {
    var entityProcedure = passedProc ?? new EntityProcedure<T>();
    entityProcedure.SetDbContext(dbContext).SetProcedure(procedureName);
    return entityProcedure;
  }

  [Obsolete("CALL is better than LOAD as terminology for executing a stored procedure")]
  public static IEntityProcedure<T> LoadProcedure<T>(
    this IAppDbContext dbContext,
    string procedureName
  )
    where T : class
  {
    var entityProcedure = new EntityProcedure<T>(dbContext, procedureName);
    return entityProcedure;
  }

  public static IEntityProcedure<T> WithParameter<T>(
    this IEntityProcedure<T> ep,
    string paramName,
    object paramValue
  )
    where T : class
  {
    ep.AddParameter(paramName, paramValue);
    return ep;
  }

  public static IEntityProcedure<T> WithParameters<T>(
    this IEntityProcedure<T> ep,
    object[] paramValues
  )
    where T : class
  {
    foreach (var t in paramValues)
    {
      ep.Add(t);
    }

    return ep;
  }

  public static T ClearTable<T, TProp>(this T dbContext, Expression<Func<T, TProp>> expression)
    where T : IAppDbContext
  {
    var tableName = ((MemberExpression)expression.Body).Member.Name;
    var strategy = Strategies.SingleOrDefault(x =>
      x.Supports(dbContext.Database.ProviderName)
    );

    if (strategy is null)
    {
      throw new NotSupportedException(
        $"Clearing tables is not supported for provider '{dbContext.Database.ProviderName}'."
      );
    }

    strategy.Reset(dbContext.Database, tableName);
    return dbContext;
  }
}

internal interface ITableResetStrategy
{
  bool Supports(string? providerName);

  void Reset(DatabaseFacade database, string tableName);
}

internal sealed class SqliteTableResetStrategy : ITableResetStrategy
{
  public bool Supports(string? providerName) =>
    providerName == "Microsoft.EntityFrameworkCore.Sqlite";

  public void Reset(DatabaseFacade database, string tableName)
  {
    var helper = database.GetService<ISqlGenerationHelper>();
    var identifier = helper.DelimitIdentifier(tableName);

    var deleteSql = $"DELETE FROM {identifier};";
    database.ExecuteSqlRaw(deleteSql);

    const string updateSequenceSql =
      "UPDATE sqlite_sequence SET seq = 1 WHERE name = {0};";
    database.ExecuteSqlRaw(updateSequenceSql, tableName);
  }
}

internal sealed class SqlServerTableResetStrategy : ITableResetStrategy
{
  public bool Supports(string? providerName) =>
    providerName == "Microsoft.EntityFrameworkCore.SqlServer";

  public void Reset(DatabaseFacade database, string tableName)
  {
    var helper = database.GetService<ISqlGenerationHelper>();
    var identifier = helper.DelimitIdentifier(tableName);

    var deleteSql = $"DELETE FROM {identifier};";
    database.ExecuteSqlRaw(deleteSql);

    var resetIdentitySql = $"DBCC CHECKIDENT ('{identifier}', RESEED, 1);";
    database.ExecuteSqlRaw(resetIdentitySql);
  }
}
