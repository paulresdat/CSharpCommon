namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class SqlScriptViewAttribute : SqlScriptIdentifierAttribute
{
    public SqlScriptViewAttribute(string fileName) : base(fileName, fileName, SqlScriptLoader.Types.Views)
    {
    }

    public SqlScriptViewAttribute(string fileName, string viewName) : base(fileName, viewName, SqlScriptLoader.Types.Views)
    {
    }

    public override SqlScriptAttributeType AttributeType => SqlScriptAttributeType.SqlScriptLoader;
}