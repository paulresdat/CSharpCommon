using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Csharp.Common.Utilities.ArgumentParsing;

/// <summary>
/// Argument parsing
///
/// 1. Extend ArgumentParsing
/// 2. Set the options
/// 3. Set the helper header if you want extra verbiage for the help screen
/// 4. Run `ParseArguments(string[] args)` and pass the args from your Program.cs file
/// 5. Use your parent class for holding the variables being set in your options to dictate what happens
///    in your Program.cs logic, or main entry point logics
/// </summary>
public abstract class ArgumentParsing
{
    // Public so that you can check if help has been called
    // and immediately quit
    public bool Help { get; set; } = false;

    protected OptionSet? Options { get; set; }
    private Action? HelpHeader { get; set; }

    protected virtual void SetOptions(OptionSet optionSet)
    {
        Options = optionSet;
        Options.Add("h|help", "Help", v => Help = v != null);
    }

    public virtual void ParseArguments(string[] args)
    {
        Options?.Parse(args);

        if (Help)
        {
            ShowHelp(Options);
        }
    }

    public void SetHelpHeader(Action helpHeaderAction)
    {
        HelpHeader = helpHeaderAction;
    }
    
    private void ShowHelp(OptionSet? p)
    {
        HelpHeader?.Invoke();
        Console.WriteLine();
        Console.WriteLine("Options: ");
        p?.WriteOptionDescriptions(Console.Out);
    }
}


/// <summary>
/// The new way of argument parsing, where it uses a passed object to interpret
/// what the arguments should be based on the property types and custom attribute
/// meta descriptions provided by it.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ArgumentParsing<T> : ArgumentParsing where T: ArgumentParsingDto, new()
{
    [Obsolete("A direct call to help is not supported when parsing arguments using a declarative class")]
    public new bool Help => throw new InvalidOperationException();
    public bool AskedForHelp => Args.Help;

    private T? _parsedArgs;
    public T Args
    {
        get => _parsedArgs ?? throw new InvalidOperationException();
    }

    private Dictionary<string, CustomCallback> Callbacks { get; } = new();

    protected abstract void SetCustomCallbacks();

    public override void ParseArguments(string[] args)
    {
        SetCustomCallbacks();
        SetOptions();
        _parsedArgs = Activator.CreateInstance<T>();
        base.ParseArguments(args);
    }

    private string ParameterizePropName(string propName)
    {
        var n = Regex.Replace(propName, @"\B([A-Z])", m => "-" + m.ToString().ToLower());
        return n.ToLower();
    }

    private Action<string> GetInternalCallback(PropertyInfo prop, ValueType valueType)
    {
        switch (valueType)
        {
            case ValueType.String:
                return (s) =>
                {
                    prop.SetValue(Args, s);
                };
            case ValueType.Int:
                return (s) =>
                {
                    int.TryParse(s, out var i);
                    prop.SetValue(Args, i);
                };
            case ValueType.Boolean:
                return (s) =>
                {
                    prop.SetValue(Args, true);
                };
            case ValueType.Byte:
                return (s) =>
                {
                    byte.TryParse(s, out var i);
                    prop.SetValue(Args, i);
                };
            case ValueType.Short:
                return (s) =>
                {
                    short.TryParse(s, out var i);
                    prop.SetValue(Args, i);
                };
            case ValueType.Long:
                return (s) =>
                {
                    short.TryParse(s, out var i);
                    prop.SetValue(Args, i);
                };
            default:
                throw new InvalidOperationException("Unsupported value type: " + valueType);
        }
    }

    protected override void SetOptions(OptionSet optionSet)
    {
        throw new InvalidOperationException(
            "The direct way of setting options is not supported using the declarative class type");
    }

    private void SetOptions()
    {
        var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var optionSet = new OptionSet();
        foreach (var prop in props)
        {
            var commandType = GetValueType(prop);
            var commandDesc = 
                prop.GetCustomAttribute<ArgumentDefinitionAttribute>() ??
                    new ArgumentDefinitionAttribute(
                        ParameterizePropName(prop.Name) + (commandType != ValueType.Boolean ? "=" : ""), prop.Name);
            CustomCallback callback;

            if (commandType != ValueType.Unsupported)
            {
                var call = GetInternalCallback(prop, commandType);
                callback = new CustomCallback();
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

    protected ArgumentParsing<T> SetCustomCallback<TProp>(Expression<Func<T, TProp>> expression, Action<T> action)
    {
        var name = ((MemberExpression) expression.Body).Member.Name;
        var customCallback = new CustomCallback();
        customCallback.Set(action);
        Callbacks[name] = customCallback;
        return this;
    }

    protected ArgumentParsing<T> SetCustomCallback<TProp>(Expression<Func<T, TProp>> expression, Action<T, string> action)
    {
        var name = ((MemberExpression) expression.Body).Member.Name;
        var customCallback = new CustomCallback();
        customCallback.Set(action);
        Callbacks[name] = customCallback;
        return this;
    }

    private ValueType GetValueType(PropertyInfo prop)
    {
        if (prop.PropertyType == typeof(string))
        {
            return ValueType.String;
        }
        
        if (prop.PropertyType == typeof(bool))
        {
            return ValueType.Boolean;
        }

        if (prop.PropertyType.GetTypeInfo() == typeof(Byte))
        {
            return ValueType.Byte;
        }

        if (prop.PropertyType.GetTypeInfo() == typeof(Int16))
        {
            return ValueType.Short;
        }

        if (prop.PropertyType.GetTypeInfo() == typeof(Int32))
        {
            return ValueType.Int;
        }

        if (prop.PropertyType.GetTypeInfo() == typeof(Int64))
        {
            return ValueType.Long;
        }

        return ValueType.Unsupported;
    }

    private enum ValueType
    {
        Byte,
        Short,
        Int,
        Long,
        String,
        Boolean,
        Unsupported,
    }
    
    private enum CallbackType
    {
        Action,
        ActionParam,
        Internal,
    }

    private class CustomCallback
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
}

public class ArgumentDefinitionAttribute : Attribute
{
    public string ArgumentName { get; set; }
    public string Description { get; set; }

    public ArgumentDefinitionAttribute(string name, string description)
    {
        // probably should validate but none for now
        ArgumentName = name;
        Description = description;
    }
}

public abstract class ArgumentParsingDto
{
    [ArgumentDefinition("h|help", "Help screen")]
    public bool Help { get; set; }
}