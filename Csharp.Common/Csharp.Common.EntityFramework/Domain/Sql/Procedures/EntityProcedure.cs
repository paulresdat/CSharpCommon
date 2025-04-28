using System.Data.SqlClient;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Csharp.Common.EntityFramework.Domain.Sql.Procedures;

public class EntityProcedure: IEntityProcedure
{
    private IAppDbContext? _dbContext;
    private IAppDbContext DbContext => _dbContext ?? throw new InvalidOperationException();
    private string? _dbProcedure;
    private string DbProcedure => _dbProcedure ?? throw new InvalidOperationException();

    private Dictionary<string, object> ParameterValues { get; set; } = new();
    private List<object> ObjectList { get; set; } = new();

    private object? _currenType = null;
    private object? _dbSet = null;

    public EntityProcedure(IAppDbContext dbContext, string dbProcedure)
    {
        _dbContext = dbContext;
        _dbProcedure = dbProcedure;
    }

    public EntityProcedure()
    {

    }

    public IEntityProcedure SetDbContext(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
        return this;
    }

    public IEntityProcedure SetProcedure(string dbProcedure)
    {
        _dbProcedure = dbProcedure;
        return this;
    }

    private DbSet<T> DbSet<T>() where T : class
    {
        if (_dbSet is not null && _currenType is not null && (Type) _currenType == typeof(T))
        {
            return (DbSet<T>) _dbSet;
        }

        var prop = _dbContext?.GetType().GetProperties()
            .First(x => x.PropertyType == typeof(DbSet<T>));

        _dbSet = (DbSet<T>?)prop?.GetValue(_dbContext) ?? throw new InvalidOperationException();
        _currenType = typeof(T);
        return (DbSet<T>)_dbSet;
    }

    #region public fluency methods
    public void Add(object paramValue)
    {
        ObjectList.Add(paramValue);
    }

    public IEntityProcedure AddParameter(string paramName, object? paramValue)
    {
        var param = paramValue ?? DBNull.Value;
        ParameterValues.Add(paramName, param);
        return this;
    }

    public IEntityProcedure WithParameters<TParams>(TParams paramObject) where TParams : class, new()
    {
        var props = paramObject.GetType().GetProperties();
        foreach (var prop in props)
        {
            ParameterValues.Add(prop.Name, prop.GetValue(paramObject) ?? DBNull.Value);
        }

        return this;
    }
    #endregion

    #region execution methods
    private IQueryable<T> PrepareProcedureAndExecute<T>() where T: class
    {
        if (ParameterValues.Count > 0)
        {
            return PrepareParameterizedProcedure<T>();
        }

        return PrepareProcedureWithoutParameterNames<T>();
    }

    private IQueryable<T> PrepareProcedureWithoutParameterNames<T>() where T: class
    {
        var sqlQuery = GenerateSqlString();
        var returnData = DbSet<T>().FromSqlRaw(sqlQuery.SqlString, ObjectList.ToArray());

        // ignore nullable context
        // TODO - fix return to ensure IQueryable<T> return type without nulls
        return returnData!;
    }

    public class SqlTrace
    {
        public string? Sql { get; set; }
        public List<object>? CurrentParameterList { get; set; }
    }

    public SqlTrace Dump()
    {
        if (ParameterValues.Count == 0)
        {
            var sql = GenerateSqlString();
            return new SqlTrace
            {
                Sql = sql.SqlString,
            };
        }
        else
        {
            var sql = GenerateSqlString(true);
            return new SqlTrace
            {
                Sql = sql.SqlString,
                CurrentParameterList = sql.ParameterList,
            };
        }
    }

    private record SqlGenerated
    {
        public string SqlString { get; set; } = "";
        public List<object> ParameterList { get; set; } = new();
    }

    private string? _lastSqlQuery;

    private SqlGenerated GenerateSqlString(bool withParameters = false)
    {
        if (!withParameters)
        {
            var methodString = "EXECUTE " + _dbProcedure + " ";
            var dlist = new List<string>();

            for (var i = 0; i < ObjectList.Count; i++)
            {
                dlist.Add("{" + i + "}");
            }

            var sqlQuery = methodString + string.Join(", ", dlist);
            _lastSqlQuery = sqlQuery;
            return new SqlGenerated
            {
                SqlString = sqlQuery,
            };
        }
        else
        {
            var methodString = "EXECUTE " + _dbProcedure + " ";
            var namedParameterList = new List<string>();
            var parameterList = new List<object>();

            foreach (var kpair in ParameterValues)
            {
                namedParameterList.Add("@" + kpair.Key + "=" + FormatParameter(kpair.Value));
                parameterList.Add(new SqlParameter(kpair.Key, kpair.Value));
            }

            var sqlQuery = methodString + string.Join(", ", namedParameterList);

            return new SqlGenerated
            {
                SqlString = sqlQuery,
                ParameterList = parameterList,
            };
        }
    }

    private object FormatParameter(object parameter)
    {
        if (parameter is string)
        {
            return "'" + parameter + "'";
        }

        if (parameter is DateTime)
        {
            throw new NotImplementedException("DateTime not accepted as a parameter at this time");
        }

        return parameter;
    }

    private IQueryable<T> PrepareParameterizedProcedure<T>() where T: class
    {
        var sql = GenerateSqlString(true);
        var dbSet = DbSet<T>();
        var returnData = dbSet.FromSqlRaw(sql.SqlString, sql.ParameterList.ToArray());

        return returnData;
    }
    #endregion

    #region projection and queryable methods
    public TDest? ProjectToFirstOrDefault<TSource, TDest>(MapperConfiguration mapperConfig)
        where TDest : class
        where TSource : class
    {
        var queryable =  PrepareProcedureAndExecute<TSource>().ToList();

        return queryable.AsQueryable<TSource>()
            .ProjectTo<TDest>(mapperConfig)
            // ef wraps a query called from FirstOrDefault in a select statement and that's not suitable for a proc
            // therefore we must fetch from the database as a list and then project to first or default
            .ToList()
            .FirstOrDefault();
    }

    public virtual IQueryable<TSource> AsQueryable<TSource>() where TSource : class
    {
        return PrepareProcedureAndExecute<TSource>();
    }

    public List<TDest> ProjectToList<TSource, TDest>(MapperConfiguration mapperConfig)
        where TSource : class
        where TDest : class
    {
        var queryable = PrepareProcedureAndExecute<TSource>().ToList();

        return queryable.AsQueryable()
            .ProjectTo<TDest>(mapperConfig)
            .ToList();
    }
    #endregion
}

public class EntityProcedure<TSource> : EntityProcedure, IEntityProcedure<TSource> where TSource : class
{
    public EntityProcedure(IAppDbContext vsiDbContext, string dbProcedure) : base(vsiDbContext, dbProcedure)
    {
    }

    public EntityProcedure() : base()
    {

    }

    public List<TDest> ProjectToList<TDest>(MapperConfiguration mapperConfig) where TDest : class
    {
        return base.ProjectToList<TSource, TDest>(mapperConfig);
    }

    public IQueryable<TSource> AsQueryable()
    {
        return base.AsQueryable<TSource>();
    }

    public TDest? ProjectToFirstOrDefault<TDest>(MapperConfiguration mapperConfig) where TDest : class
    {
        return base.ProjectToFirstOrDefault<TSource, TDest>(mapperConfig);
    }
}
