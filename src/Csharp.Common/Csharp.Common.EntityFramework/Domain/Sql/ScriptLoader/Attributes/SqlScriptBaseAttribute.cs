namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader.Attributes;

public interface ISqlScriptAttribute
{
    SqlScriptBaseAttribute.SqlScriptAttributeType AttributeType { get; }
}

public abstract class SqlScriptBaseAttribute : Attribute, ISqlScriptAttribute
{
    public enum SqlScriptAttributeType
    {
        SqlScriptLoader,
        SqlScriptInterpolator,
    }

    public abstract SqlScriptAttributeType AttributeType { get; }
}


public abstract class SqlScriptIdentifierAttribute : SqlScriptBaseAttribute
{
    public SqlScriptLoader.Types ScriptType { get; set; }
    public string FileName { get; set; }
    public string ViewName { get; set; }

    public SqlScriptIdentifierAttribute(string fileName, string? viewName, SqlScriptLoader.Types scriptType)
    {
        FileName = fileName;
        ViewName = viewName ?? fileName;
        ScriptType = scriptType;
    }
}