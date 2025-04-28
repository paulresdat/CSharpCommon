namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;

public abstract class SqlScriptInterpolator
{
    protected abstract string Interpolate(string fileName, SqlScriptLoader.Types sqlScriptLoaderType,
        string fileContent);

    public Func<string, SqlScriptLoader.Types, string, string> GetInterpolator() => Interpolate;
}