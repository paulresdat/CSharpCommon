namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class SqlScriptInterpolatorAttribute : SqlScriptBaseAttribute
{
    private readonly Type _interpolatorType;
    public SqlScriptInterpolatorAttribute(Type interpolatorType)
    {
        if (!interpolatorType.IsSubclassOf(typeof(SqlScriptInterpolator)))
        {
            throw new InvalidOperationException(
                "Sql Script Interpolator requires to be a subtype of SqlScriptInterpolator");
        }
        _interpolatorType = interpolatorType;
    }

    public override SqlScriptAttributeType AttributeType => SqlScriptAttributeType.SqlScriptInterpolator;
    public Type InterpolatorType => _interpolatorType;
}
