using Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader.Attributes;

namespace Csharp.Common.EntityFramework.Domain.Sql.ScriptLoader;

public abstract class SqlScriptDetail
{
    public static SqlScriptDetail Create(Type classType, List<Attribute> attributes)
    {
        var identifier = GetAttribute<SqlScriptIdentifierAttribute>(attributes);
        return identifier.ScriptType switch
        {
            SqlScriptLoader.Types.Views => new SqlScriptViewDetail
            {
                ClassType = classType,
                Attributes = attributes,
            },
            SqlScriptLoader.Types.Procedures => new SqlScriptProcDetail
            {
                ClassType = classType,
                Attributes = attributes,
            },
            SqlScriptLoader.Types.ServiceBrokers => new SqlScriptServiceBroker
            {
                ClassType = classType,
                Attributes = attributes,
            },
            SqlScriptLoader.Types.Functions => new SqlScriptFunction
            {
                ClassType = classType,
                Attributes = attributes,
            },
            SqlScriptLoader.Types.Triggers => new SqlScriptTrigger
            {
                ClassType = classType,
                Attributes = attributes,
            },
            SqlScriptLoader.Types.SpatialIndex => new SqlScriptSpatialIndex
            {
                ClassType = classType,
                Attributes = attributes,
            },
            _ => throw new NotSupportedException("The type: " + identifier.ScriptType + " is not supported"),
        };
    }

    public Type ClassType { get; set; } = null!;
    public List<Attribute> Attributes { get; set; } = new();

    private TSubclass GetAttribute<TSubclass>() where TSubclass: SqlScriptBaseAttribute
    {
        return (TSubclass)Attributes.First(x => x.GetType().IsSubclassOf(typeof(TSubclass)));
    }

    private static TSubclass GetAttribute<TSubclass>(List<Attribute> attributes) where TSubclass: SqlScriptBaseAttribute
    {
        return (TSubclass) attributes.First(x => x.GetType().IsSubclassOf(typeof(TSubclass)));
    }

    private SqlScriptIdentifierAttribute? _identifier;

    public SqlScriptIdentifierAttribute Identifier
    {
        get
        {
            if (_identifier is null)
            {
                _identifier = GetAttribute<SqlScriptIdentifierAttribute>();
            }

            return _identifier;
        }
    }

    public SqlScriptLoader.Types IdentifierType => Identifier.ScriptType;

    public List<SqlScriptInterpolatorAttribute> InterpolatorAttributes =>
        Attributes.Where(x => x.GetType() == typeof(SqlScriptInterpolatorAttribute))
            .Cast<SqlScriptInterpolatorAttribute>().ToList();

    // there might be a more elegant way of doing this
    public bool HasInterpolatorAttribute => Attributes.Any(x => x.GetType() == typeof(SqlScriptInterpolatorAttribute));

    public bool Is<TClassType>() => typeof(TClassType) == ClassType;

    public bool IsView => Identifier.ScriptType == SqlScriptLoader.Types.Views;
    public bool IsProc => Identifier.ScriptType == SqlScriptLoader.Types.Procedures;
    public bool IsServiceBrokers => Identifier.ScriptType == SqlScriptLoader.Types.ServiceBrokers;
    public bool IsFunctions => Identifier.ScriptType == SqlScriptLoader.Types.Functions;
    public bool IsTriggers => Identifier.ScriptType == SqlScriptLoader.Types.Triggers;
    public bool IsSpatialIndex => Identifier.ScriptType == SqlScriptLoader.Types.SpatialIndex;
}

public class SqlScriptViewDetail : SqlScriptDetail { }
public class SqlScriptProcDetail : SqlScriptDetail { }
public class SqlScriptServiceBroker : SqlScriptDetail { }
public class SqlScriptFunction : SqlScriptDetail { }
public class SqlScriptTrigger : SqlScriptDetail { }
public class SqlScriptSpatialIndex : SqlScriptDetail { }