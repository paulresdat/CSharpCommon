using System.Linq.Expressions;
using Csharp.Common.EntityFramework.Domain;
using Csharp.Common.EntityFramework.Domain.Sql.Procedures;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Common.EntityFramework.Extensions;

public static class AppDbContextExtensions
{
    public static IEntityProcedure CallProcedure(this IAppDbContext dbContext, string procedureName, IEntityProcedure? passedProc = null)
    {
        var entityProcedure = passedProc ?? new EntityProcedure();
        entityProcedure.SetDbContext(dbContext).SetProcedure(procedureName);
        return entityProcedure;
    }

    public static IEntityProcedure<T> CallProcedure<T>(this IAppDbContext dbContext, string procedureName, IEntityProcedure<T>? passedProc = null) where T : class
    {
        var entityProcedure = passedProc ?? new EntityProcedure<T>();
        entityProcedure.SetDbContext(dbContext).SetProcedure(procedureName);
        return entityProcedure;
    }

    [Obsolete("CALL is better than LOAD as terminology for executing a stored procedure")]
    public static IEntityProcedure<T> LoadProcedure<T>(this IAppDbContext dbContext, string procedureName) where T : class
    {
        var entityProcedure = new EntityProcedure<T>(dbContext, procedureName);
        return entityProcedure;
    }

    public static IEntityProcedure<T> WithParameter<T>(this IEntityProcedure<T> ep, string paramName, object paramValue) where T : class
    {
        ep.AddParameter(paramName, paramValue);
        return ep;
    }

    public static IEntityProcedure<T> WithParameters<T>(this IEntityProcedure<T> ep, object[] paramValues) where T : class
    {
        foreach (var t in paramValues)
        {
            ep.Add(t);
        }

        return ep;
    }

    public static T ClearTable<T, TProp>(this T dbContext, Expression<Func<T, TProp>> expression) where T : IAppDbContext
    {
        var typeName = dbContext.Database.GetDbConnection().GetType().Name;
        var tableName = ((MemberExpression) expression.Body).Member.Name;
        if (typeName == "SqliteConnection")
        {
            // for now just truncate
            dbContext.Database.ExecuteSqlRaw($"delete from {tableName}");
            dbContext.Database.ExecuteSqlRaw($"update sqlite_sequence set seq=1 where name = '{tableName}'");
        }
        else
        {
            dbContext.Database.ExecuteSqlRaw($"DELETE FROM [{tableName}];");
            dbContext.Database.ExecuteSqlRaw($"DBCC CHECKIDENT ('{tableName}', RESEED, 1);");
        }
        return dbContext;
    }
}
