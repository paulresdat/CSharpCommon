namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader.Attributes;

public sealed class SqlScriptTriggerAttribute : SqlScriptIdentifierAttribute
{
    public SqlScriptTriggerAttribute(string fileName) : base(fileName, fileName, SqlScriptLoader.Types.Triggers)
    {
    }

    public SqlScriptTriggerAttribute(string fileName, string viewName) : base(fileName, viewName, SqlScriptLoader.Types.Triggers)
    {
    }

    public override SqlScriptAttributeType AttributeType => SqlScriptAttributeType.SqlScriptLoader;
}