namespace Csharp.Common.EntityFramework.Domain.Sql.Procedures;

/// <summary>
/// Used in conjunction with ProcColumnAttribute
///
/// Use fort AkrrDbContext to automagically understand and map procs to a concrete.  You can
/// then use the ProcDbSet[T] in your DbContext and access procedures must the same way
/// as other DbSets[T] in Entity Framework.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class IsProcedureAttribute : Attribute
{
    public string? ProcName { get; set; }
    public IsProcedureAttribute()
    {
    }

    public IsProcedureAttribute(string procName)
    {
        ProcName = procName;
    }
}
