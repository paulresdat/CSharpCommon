using System.Reflection;
using Csharp.Common.EntityFramework.Domain.Sql.Procedures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Csharp.Common.EntityFramework.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder EntityFromProc(this ModelBuilder builder, string procName, Type procType,
        List<ProcAttributeDetails> atts)
    {
        builder.Entity(procType, eb =>
        {
            ProcToViewMap.Add(procType, procName);
            eb.HasNoKey();
            eb.ToView(procName);
            foreach (var att in atts)
            {
                if (att.Attribute is not null)
                {
                    eb.Property(att.Type.PropertyType, att.Attribute?.DbColumnName ?? att.Type.Name);
                }
            }
        });
        return builder;
    }

    public static ModelBuilder EntityFromProc<T>(this ModelBuilder builder, string procName, Action<EntityTypeBuilder<T>> config) where T: class
    {
        builder.Entity<T>(eb =>
        {
            ProcToViewMap.Add<T>(procName);
            eb.HasNoKey();
            eb.ToView(procName);
            config.Invoke(eb);
        });
        return builder;
    }
}

public static class ProcToViewMap
{
    private static readonly Dictionary<Type, string> ProcToViewDictionary = new();

    public static void Add(Type procType, string procName)
    {
        if (ProcToViewDictionary.ContainsKey(procType))
        {
            if (ProcToViewDictionary[procType] != procName)
            {
                throw new InvalidOperationException(
                    "Same type " + procType.Name + " references 2 different procs: " +
                    ProcToViewDictionary[procType] + " : " + procName);
            }
        }

        ProcToViewDictionary.Add(procType, procName);
    }

    public static void Add<T>(string procName)
    {
        Add(typeof(T), procName);
    }

    public static string ProcNameOfType<T>()
    {
        if (ProcToViewDictionary.ContainsKey(typeof(T)))
        {
            return ProcToViewDictionary[typeof(T)];
        }

        throw new InvalidOperationException(
            "Proc to view was not mapped, did you use the model build extenstion EntityFromProc?");
    }
}

public class ProcAttributeDetails
{
    public PropertyInfo Type { get; set; } = null!;
    public ProcColumnAttribute? Attribute { get; set; } = null!;
}
