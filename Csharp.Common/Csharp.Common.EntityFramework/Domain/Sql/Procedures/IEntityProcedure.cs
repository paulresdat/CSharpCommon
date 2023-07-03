using AutoMapper;
using Csharp.Common.Utilities;

namespace Csharp.Common.EntityFramework.Domain.Sql.Procedures;

public interface IEntityProcedure : ITransientService
{
    IEntityProcedure SetDbContext(IAppDbContext dbContext);
    IEntityProcedure SetProcedure(string dbProcedure);
    void Add(object paramValue);
    IEntityProcedure AddParameter(string paramName, object? paramValue);
    IEntityProcedure WithParameters<TParams>(TParams paramObject) where TParams : class, new();
    TDest? ProjectToFirstOrDefault<TSource, TDest>(MapperConfiguration mapperConfig)
        where TDest : class
        where TSource : class;
    IQueryable<TSource> AsQueryable<TSource>() where TSource : class;
    List<TDest> ProjectToList<TSource, TDest>(MapperConfiguration mapperConfig)
        where TSource : class
        where TDest : class;
    EntityProcedure.SqlTrace Dump();
}

public interface IEntityProcedure<TSource> : IEntityProcedure where TSource : class
{
    List<TDest> ProjectToList<TDest>(MapperConfiguration mapperConfig) where TDest : class;
    IQueryable<TSource> AsQueryable();
    TDest? ProjectToFirstOrDefault<TDest>(MapperConfiguration mapperConfig) where TDest : class;
}
