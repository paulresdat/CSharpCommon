namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;

/// <summary>
/// Reverses things here and allows for reflection look up to automatically load views/procedures
/// </summary>
public class SqlScriptLoaderFactory
{
    private class ScriptAttributesDto
    {
        public Type ClassType { get; set; } = null!;
        // public List<
    }


    public static SqlScriptLoader New(string? sqlScriptLoaderStartingPath = null)
    {
        var sqlScriptLoader = new SqlScriptLoader(sqlScriptLoaderStartingPath ?? string.Empty);
        // var classAttributes = GetClassAttributes(viewType);
        return sqlScriptLoader;
    }
}