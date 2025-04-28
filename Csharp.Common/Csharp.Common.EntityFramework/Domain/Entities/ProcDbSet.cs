using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using Csharp.Common.EntityFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Csharp.Common.EntityFramework.Domain.Entities;

public class ProcDbSet<T> :
    IQueryable<T>,
    IEnumerable<T>,
    IEnumerable,
    IQueryable,
    IInfrastructure<IServiceProvider>,
    IListSource
    where T : class
{
    private Dictionary<string, object?> ParameterValues { get; set; } = new();
    private readonly IAppDbContext _context;
    public DbSet<T> DbSet { get; set; }

    public ProcDbSet(DbSet<T> dbSet, IAppDbContext context)
    {
        _context = context;
        DbSet = dbSet;
    }

    private object? Params { get; set; }

    public ProcDbSet<T> WithParameters<TParams>(TParams paramObject) where TParams : class, new()
    {
        Params = paramObject;
        return this;
    }

    public ProcDbSet<T> WithParameter(string parameterName, object? value)
    {
        ParameterValues.Add(parameterName, value);
        return this;
    }

    private List<T> CallProc()
    {
        var proc = _context.CallProcedure(ProcToViewMap.ProcNameOfType<T>());
        if (Params is not null)
        {
            proc.WithParameters(Params);
            Params = null;
        }

        if (ParameterValues.Count > 0)
        {
            foreach (var param in ParameterValues)
            {
                proc.AddParameter(param.Key, param.Value);
            }
            ParameterValues.Clear();
        }

        return proc.AsQueryable<T>().ToList();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return CallProc().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // TODO - implement more thoroughly as time goes on
    public Type ElementType { get; } = null!;
    public Expression Expression { get; } = null!;
    public IQueryProvider Provider { get; } = null!;
    public IServiceProvider Instance { get; } = null!;

    public IList GetList()
    {
        throw new NotImplementedException();
    }

    public bool ContainsListCollection { get; } = false;
}
