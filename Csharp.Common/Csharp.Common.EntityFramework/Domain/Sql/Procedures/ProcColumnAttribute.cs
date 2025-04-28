using System.Data;

namespace Csharp.Common.EntityFramework.Domain.Sql.Procedures;

[AttributeUsage(AttributeTargets.Property)]
public class ProcColumnAttribute : Attribute
{
    public string? DbColumnName { get; set; }
    public DbType? DbType { get; set; }

    public ProcColumnAttribute()
    {
    }

    public ProcColumnAttribute(string dbColumnName)
    {
        DbColumnName = dbColumnName;
    }

    public ProcColumnAttribute(DbType dbType)
    {
        DbType = dbType;
    }

    public ProcColumnAttribute(string dbColumnName, DbType dbType)
    {
        DbColumnName = dbColumnName;
        DbType = dbType;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class ProcColumnIgnoreAttribute : Attribute
{
}