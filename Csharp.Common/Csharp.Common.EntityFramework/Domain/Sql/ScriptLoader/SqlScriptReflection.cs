using System.Reflection;
using Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader.Attributes;

namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;

public static class SqlScriptReflection
{
    public static SqlScriptDetail GetDetailFromType(Type type)
    {
        var attributes = type
            .GetCustomAttributes()
            .Where(x => x.GetType().IsAssignableTo(typeof(ISqlScriptAttribute)))
            .ToList();

        return SqlScriptDetail.Create(type, attributes);
    }

    public static SqlScriptDetailList RetrieveAllSqlScripts<TAssembly>()
    {
        return new SqlScriptDetailList(RetrieveAllSqlScripts(typeof(TAssembly)));
    }

    public static List<SqlScriptDetail> RetrieveAllSqlScripts(Type assembly)
    {
        return assembly.Assembly.GetTypes()
            .Where(x => x.IsClass)
            .Where(t => t
                .GetCustomAttributes()
                .Any(x => x.GetType().IsAssignableTo(typeof(ISqlScriptAttribute))))
            .Select(t =>
            {
                var attributes = t.GetCustomAttributes()
                    .Where(y => y.GetType().IsAssignableTo(typeof(ISqlScriptAttribute)))
                    .ToList();
                return SqlScriptDetail.Create(t, attributes);
            })
            .ToList();
    }

    public static List<SqlScriptDetail> GetAllClassesAndTheirAttributes<T>(Type assembly)
    {
        return assembly.Assembly.GetTypes()
            .Where(x => x.IsClass)
            .Where(t => t
                .GetCustomAttributes()
                .Any(x => x.GetType() == typeof(T)))
            .Select(t =>
            {
                var atts = t.GetCustomAttributes()
                    .Where(y => y.GetType().IsAssignableFrom(typeof(ISqlScriptAttribute)))
                    .ToList();
                return SqlScriptDetail.Create(t, atts);
            })
            .ToList();
    }
}