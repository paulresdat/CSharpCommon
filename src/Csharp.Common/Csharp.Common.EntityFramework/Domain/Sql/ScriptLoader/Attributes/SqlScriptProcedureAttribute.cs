namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SqlScriptProcedureAttribute : SqlScriptIdentifierAttribute
{
    public SqlScriptProcedureAttribute(string procName) : base(procName, procName, SqlScriptLoader.Types.Procedures)
    {
    }

    public SqlScriptProcedureAttribute(string procFileName, string procName) : base(procFileName, procName, SqlScriptLoader.Types.Procedures)
    {
    }

    public override SqlScriptAttributeType AttributeType => SqlScriptAttributeType.SqlScriptLoader;
}