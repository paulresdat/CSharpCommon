using System.Linq.Expressions;
using System.Reflection;

namespace Csharp.Common.Utilities.ArgumentParsing;

public abstract class ArgumentParser
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="args"></param>
    /// <typeparam name="TDto"></typeparam>
    /// <returns></returns>
    public static TDto Parse<TDto>(string[] args)
        where TDto : ArgumentParsingDto, new()
    {
        var argParse = new ArgumentParser<TDto>();
        argParse.ParseArguments(args);
        return argParse.Args;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="args"></param>
    /// <param name="setCustomInterpreters"></param>
    /// <typeparam name="TDto"></typeparam>
    /// <returns></returns>
    public static TDto Parse<TDto>(string[] args, Action<IArgumentParserCustomInterpreting<TDto>> setCustomInterpreters)
        where TDto : ArgumentParsingDto, new()
    {
        var argParse = new ArgumentParser<TDto>();
        setCustomInterpreters.Invoke(argParse);
        argParse.ParseArguments(args);
        return argParse.Args;
    }
}

/// <include file='../../Documentation/arg-parser.xml' path='extradoc/class[@name="IArgumentParserCustomInterpreting:T"]/*'/>
public interface IArgumentParserCustomInterpreting<T> where T : ArgumentParsingDto, new()
{
    /// <summary>
    /// CustomInterpreter allow for direct control over how the value that is supplied on the command line is assigned
    /// to the property.  The library automatically all major data types and enums, so this functionality may not
    /// be required for use in your project.
    /// </summary>
    /// <param name="expression">The target property</param>
    /// <param name="action">Custom interpretation action</param>
    /// <typeparam name="TProp">The data type of the property</typeparam>
    /// <returns></returns>
    ArgumentParser<T> CustomInterpreter<TProp>(Expression<Func<T, TProp>> expression, Func<string, TProp> action);
}

/// <include file='../../Documentation/arg-parser.xml' path='extradoc/class[@name="ArgumentParser:T"]/primarydoc/*'/>
public class ArgumentParser<T> : IArgumentParserCustomInterpreting<T> where T: ArgumentParsingDto, new()
{
    private Action? HelpHeader { get; set; }
    private Dictionary<string, InternalCallback> Callbacks { get; } = new();
    protected OptionSet? Options { get; set; }
    public bool AskedForHelp => Args.Help;
    private IConsoleOutput? Output { get; set; }

    private T? _parsedArgs;
    public T Args
    {
        get => _parsedArgs ?? throw new InvalidOperationException();
    }

    /// <summary>
    /// Overrides Console with IConsoleOutput for injection and testing.
    /// </summary>
    /// <param name="consoleOutput">IConsoleOuput requested override</param>
    public void SetOutput(IConsoleOutput consoleOutput)
    {
        Output = consoleOutput;
    }

    /// <summary>
    /// Parses the arguments of a string using the Options.cs suite.
    /// </summary>
    /// <param name="args">The array of string values supplied from the command line</param>
    public void ParseArguments(string[] args)
    {
        SetOptions();
        _parsedArgs = Activator.CreateInstance<T>();
        try
        {
            Options?.Parse(args);
        }
        catch (OptionException e)
        {
            WriteLine(e.Message);
            WriteLine("Invalid argument detected.");
            return;
        }

        if (Args.Help)
        {
            ShowHelp(Options);
        }
    }

    /// <summary>
    /// See documentation on interface.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="returnedData"></param>
    /// <typeparam name="TProp"></typeparam>
    /// <returns></returns>
    public ArgumentParser<T> CustomInterpreter<TProp>(Expression<Func<T, TProp>> expression, Func<string, TProp> returnedData)
    {
        var name = ((MemberExpression) expression.Body).Member.Name;
        var customCallback = new InternalCallback();
        customCallback.Set((arg, s) =>
        {
            var data = returnedData.Invoke(s);
            var prop = arg.GetType().GetProperties().First(x => x.Name == name);
            prop.SetValue(arg, data);
        });
        Callbacks[name] = customCallback;
        return this;
    }

    #region argument parsing

    private string StandardizeCommand(string command, bool unknownType, Type propValueType)
    {
        if (command.EndsWith("="))
        {
            return command;
        }

        MappedTypes.TryGetValue(propValueType, out var detail);
        if (unknownType || (detail is not null && detail.ExpectsValue))
        {
            return command + "=";
        }

        return string.Join("", command.ToArray());
    }

    private void SetOptions()
    {
        // base.SetOptions();
        var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var optionSet = new OptionSet();

        foreach (var prop in props)
        {
            var commandType = GetValueType(prop);
            var commandDesc =
                prop.GetCustomAttribute<ArgumentDefinitionAttribute>() ??
                    throw new InvalidOperationException("all command options require an attribute defining the argument");

            InternalCallback callback;

            if (commandType != ValueType.Unsupported)
            {
                var underlyingPart = Nullable.GetUnderlyingType(prop.PropertyType);
                Type propType;
                if (underlyingPart?.IsEnum ?? false)
                {
                    propType = underlyingPart;
                }
                else
                {
                    propType = prop.PropertyType;
                }
                var call = GetInternalCallback(prop, commandType, propType);
                callback = new InternalCallback();
                callback.Set(call);
            }
            else
            {
                // check for the existence of a custom callback
                if (!Callbacks.ContainsKey(prop.Name))
                {
                    throw new InvalidOperationException("No custom callback found for prop: " + prop.Name);
                }

                callback = Callbacks.First(x => x.Key == prop.Name).Value;
            }

            optionSet.Add(commandDesc.ArgumentName, commandDesc.Description, (s) =>
            {
                switch (callback.CallbackType)
                {
                    case CallbackType.Internal:
                    {
                        var action = callback.Get<Action<string>>();
                        action.Invoke(s);
                        break;
                    }
                    case CallbackType.Action:
                    {
                        var action = callback.Get<Action<T>>();
                        action.Invoke(Args);
                        break;
                    }
                    case CallbackType.ActionParam:
                    {
                        var action = callback.Get<Action<T, string>>();
                        action.Invoke(Args, s);
                        break;
                    }

                    default:
                        throw new InvalidOperationException("Unsupported type: " + callback.CallbackType);
                }
            });
        }

        Options = optionSet;
    }
    #endregion

    #region argument interpretation

    private ValueType GetValueType(PropertyInfo prop)
    {

        var propType = prop.PropertyType;
        // may be nullable, get underlying type
        var underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);
        var propTypeInfo = prop.PropertyType.GetTypeInfo();

        if ((underlyingType ?? propType).IsEnum)
        {
            return underlyingType is not null ? ValueType.EnumNullable : ValueType.Enum;
        }

        if (MappedTypes.TryGetValue(propTypeInfo, out var detail))
        {
            return detail.ValueType;
        }

        return ValueType.Unsupported;
    }

    private Action<string> GetInternalCallback(PropertyInfo prop, ValueType valueType, Type? mapType = null)
    {
        switch (valueType)
        {
            case ValueType.String:
                return (s) =>
                {
                    prop.SetValue(Args, s);
                };
            case ValueType.Int:
            case ValueType.IntNullable:
                return (s) =>
                {
                    if (int.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Uint:
            case ValueType.UintNullable:
                return (s) =>
                {
                    if (uint.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Boolean:
            case ValueType.BooleanNullable:
                return (s) =>
                {
                    prop.SetValue(Args, true);
                };
            case ValueType.Byte:
            case ValueType.ByteNullable:
                return (s) =>
                {
                    if (byte.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Sbyte:
            case ValueType.SbyteNullable:
                return (s) =>
                {
                    if (sbyte.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Short:
            case ValueType.ShortNullable:
                return (s) =>
                {
                    if (short.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Ushort:
            case ValueType.UshortNullable:
                return (s) =>
                {
                    if (ushort.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Long:
            case ValueType.LongNullable:
                return (s) =>
                {
                    if (long.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Ulong:
            case ValueType.UlongNullable:
                return (s) =>
                {
                    if (ulong.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Double:
            case ValueType.DoubleNullable:
                return (s) =>
                {
                    if (double.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Decimal:
            case ValueType.DecimalNullable:
                return (s) =>
                {
                    if (decimal.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Float:
            case ValueType.FloatNullable:
                return (s) =>
                {
                    if (float.TryParse(s, out var i))
                    {
                        prop.SetValue(Args, i);
                    }
                };
            case ValueType.Enum:
                return (s) =>
                {
                    var t = mapType ??
                        throw new InvalidOperationException(
                            "Enum type not found when trying to call internal callback on enum type");
                    var val = Enum.Parse(t, s, true);
                    prop.SetValue(Args, val);
                };
            case ValueType.EnumNullable:
                return (s) =>
                {
                    var t = mapType ??
                        throw new InvalidOperationException(
                            "Enum type not found when trying to call internal callback on enum type");
                    if (Enum.TryParse(t, s, true, out var v))
                    {
                        prop.SetValue(Args, v);
                    }
                };
            default:
                throw new InvalidOperationException("Unsupported value type: " + valueType);
        }
    }

    private Dictionary<Type, ValueTypeDetail> MappedTypes { get; set; } = new()
    {
        { typeof(string), ValueTypeDetail.New(ValueType.String) },
        { typeof(bool), ValueTypeDetail.New(ValueType.Boolean, false) },
        { typeof(bool?), ValueTypeDetail.New(ValueType.BooleanNullable, false) },
        { typeof(double), ValueTypeDetail.New(ValueType.Double) },
        { typeof(double?), ValueTypeDetail.New(ValueType.DoubleNullable) },
        { typeof(decimal), ValueTypeDetail.New(ValueType.Decimal) },
        { typeof(decimal?), ValueTypeDetail.New(ValueType.DecimalNullable) },
        { typeof(float), ValueTypeDetail.New(ValueType.Double) },
        { typeof(float?), ValueTypeDetail.New(ValueType.FloatNullable) },
        { typeof(Byte), ValueTypeDetail.New(ValueType.Byte) },
        { typeof(SByte), ValueTypeDetail.New(ValueType.Sbyte) },
        { typeof(Byte?), ValueTypeDetail.New(ValueType.ByteNullable) },
        { typeof(SByte?), ValueTypeDetail.New(ValueType.SbyteNullable) },
        { typeof(Int16), ValueTypeDetail.New(ValueType.Short) },
        { typeof(UInt16), ValueTypeDetail.New(ValueType.Ushort) },
        { typeof(Int16?), ValueTypeDetail.New(ValueType.ShortNullable) },
        { typeof(UInt16?), ValueTypeDetail.New(ValueType.UshortNullable) },
        { typeof(Int32), ValueTypeDetail.New(ValueType.Int) },
        { typeof(UInt32), ValueTypeDetail.New(ValueType.Uint) },
        { typeof(Int32?), ValueTypeDetail.New(ValueType.IntNullable) },
        { typeof(UInt32?), ValueTypeDetail.New(ValueType.UintNullable) },
        { typeof(Int64), ValueTypeDetail.New(ValueType.Long) },
        { typeof(Int64?), ValueTypeDetail.New(ValueType.LongNullable) },
        { typeof(UInt64), ValueTypeDetail.New(ValueType.Ulong) },
        { typeof(UInt64?), ValueTypeDetail.New(ValueType.UlongNullable) },
    };

    private enum ValueType
    {
        Byte,
        Short,
        Int,
        Long,

        Sbyte,
        Ushort,
        Uint,
        Ulong,

        Double,
        Float,
        Decimal,

        String,
        Boolean,
        Unsupported,

        ByteNullable,
        ShortNullable,
        IntNullable,
        LongNullable,
        SbyteNullable,
        UshortNullable,
        UintNullable,
        UlongNullable,
        BooleanNullable,
        EnumNullable,
        FloatNullable,
        DecimalNullable,
        DoubleNullable,

        Enum,
    }

    private enum CallbackType
    {
        Action,
        ActionParam,
        Internal,
    }

    private class ValueTypeDetail
    {
        public ValueType ValueType { get; set; }
        public bool ExpectsValue { get; set; } = true;

        public static ValueTypeDetail New(ValueType valueType, bool expectsValue = true)
        {
            return new ValueTypeDetail
            {
                ValueType = valueType,
                ExpectsValue = expectsValue,
            };
        }
    }

    private class InternalCallback
    {
        private object? Action { get; set; }
        public CallbackType CallbackType { get; set; }

        public void Set(Action<string> internalCallback)
        {
            Action = internalCallback;
            CallbackType = CallbackType.Internal;
        }

        public void Set(Action<T> action)
        {
            Action = action;
            CallbackType = CallbackType.Action;
        }

        public void Set(Action<T, string> action)
        {
            Action = action;
            CallbackType = CallbackType.ActionParam;
        }

        public T2 Get<T2>()
        {
            return (T2) (Action ?? throw new InvalidOperationException("Action must be set before fetching"));
        }
    }
    #endregion argument interpretation

    #region writing to console
    private void ShowHelp(OptionSet? p)
    {
        HelpHeader?.Invoke();
        WriteLine("");
        WriteLine("Options:");
        if (Output is not null)
        {
            p?.WriteOptionDescriptions(Output);
        }
        else
        {
            p?.WriteOptionDescriptions(Console.Out);
        }
    }

    private void WriteLine(params object?[] lines)
    {
        foreach (var l in lines)
        {
            if (Output is not null)
            {
                Output?.WriteLine(l?.ToString() ?? string.Empty);
            }
            else
            {
                Console.WriteLine(l?.ToString());
            }
        }
    }
    #endregion writing to console
}
