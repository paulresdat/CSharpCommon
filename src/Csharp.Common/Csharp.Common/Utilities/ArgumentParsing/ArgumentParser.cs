using System.Linq.Expressions;
using System.Reflection;
using System.CommandLine;
using CommandLineOption = System.CommandLine.Option;

namespace Csharp.Common.Utilities.ArgumentParsing;

public abstract class ArgumentParser
{
  public static TDto Parse<TDto>(string[] args)
    where TDto : ArgumentParserDto, new()
  {
    var parser = new ArgumentParser<TDto>();
    parser.ParseArguments(args);
    return parser.Args;
  }

  public static TDto Parse<TDto>(
    string[] args,
    Action<IArgumentParserCustomInterpreting<TDto>> setCustomInterpreters
  )
    where TDto : ArgumentParserDto, new()
  {
    var parser = new ArgumentParser<TDto>();
    setCustomInterpreters(parser);
    parser.ParseArguments(args);
    return parser.Args;
  }
}

public interface IArgumentParserCustomInterpreting<T>
  where T : ArgumentParserDto, new()
{
  IArgumentParserCustomInterpreting<T> CustomInterpreter<TProp>(
    Expression<Func<T, TProp>> expression,
    Func<string, TProp> action
  );
}

public interface IArgumentParser<T> : IArgumentParserCustomInterpreting<T>
  where T : ArgumentParserDto, new()
{
  void SetOutput(IConsoleOutput consoleOutput);
  void ParseArguments(string[] args);
}

public class ArgumentParser<T> : IArgumentParser<T>
  where T : ArgumentParserDto, new()
{
  private readonly Dictionary<string, Func<string, object?>> _customInterpreters = [];
  private readonly HashSet<string> _flagCallbacks = [];
  private IConsoleOutput Output { get; set; } = ConsoleOutput.Instance;
  private T? _args;

  public bool AskedForHelp => Args.Help;

  public T Args => _args ?? throw new InvalidOperationException();

  protected virtual void SetCustomCallbacks() { }

  public void SetOutput(IConsoleOutput consoleOutput)
  {
    Output = consoleOutput;
  }

  public void ParseArguments(string[] args)
  {
    SetCustomCallbacks();
    var bindings = CreateBindings();
    var command = new RootCommand();
    foreach (var binding in bindings)
    {
      command.Add(binding.Option);
    }

    _args = new T();
    command.SetAction(parseResult => ApplyValues(parseResult, bindings));

    var parseResult = command.Parse(NormalizeFlagArguments(args, bindings));
    if (parseResult.Errors.Count > 0)
    {
      foreach (var error in parseResult.Errors)
      {
        Output.WriteLine(error.Message);
      }

      Output.WriteLine("Invalid argument detected.");
      return;
    }

    parseResult.Invoke(new InvocationConfiguration { Output = TextWriter.Null, Error = TextWriter.Null });

    if (args.Any(x => x is "-h" or "--help"))
    {
      Args.Help = true;
    }

    if (Args.Help)
    {
      WriteHelp(command);
    }
  }

  public ArgumentParser<T> CustomInterpreter<TProp>(
    Expression<Func<T, TProp>> expression,
    Func<string, TProp> returnedData
  )
  {
    var propertyName = ((MemberExpression)expression.Body).Member.Name;
    _customInterpreters[propertyName] = value => returnedData(value);
    _flagCallbacks.Remove(propertyName);
    return this;
  }

  IArgumentParserCustomInterpreting<T>
    IArgumentParserCustomInterpreting<T>.CustomInterpreter<TProp>(
      Expression<Func<T, TProp>> expression,
      Func<string, TProp> returnedData
    ) => CustomInterpreter(expression, returnedData);

  protected ArgumentParser<T> SetCustomCallback<TProp>(
    Expression<Func<T, TProp>> expression,
    Action<T> action
  )
  {
    var propertyName = ((MemberExpression)expression.Body).Member.Name;
    _customInterpreters[propertyName] = _ =>
    {
      action(Args);
      return null;
    };
    _flagCallbacks.Add(propertyName);
    return this;
  }

  protected ArgumentParser<T> SetCustomCallback<TProp>(
    Expression<Func<T, TProp>> expression,
    Action<T, string> action
  )
  {
    var propertyName = ((MemberExpression)expression.Body).Member.Name;
    _customInterpreters[propertyName] = value =>
    {
      action(Args, value);
      return null;
    };
    _flagCallbacks.Remove(propertyName);
    return this;
  }

  private IReadOnlyList<PropertyBinding> CreateBindings()
  {
    return
    [
      .. GetProperties().Select(property =>
      {
        var definition = property.GetCustomAttribute<ArgumentDefinitionAttribute>()
                         ?? throw new InvalidOperationException(
                           "all command options require an attribute defining the argument"
                         );

        var isSupported = IsSupported(property.PropertyType);
        if (!isSupported && !_customInterpreters.ContainsKey(property.Name))
        {
          throw new InvalidOperationException("No custom callback found for prop: " + property.Name);
        }

        return new PropertyBinding(
          property,
          CreateOption(property, definition, isSupported, _flagCallbacks.Contains(property.Name)),
          !isSupported,
          _flagCallbacks.Contains(property.Name)
        );
      })
    ];
  }

  private static CommandLineOption CreateOption(
    PropertyInfo property,
    ArgumentDefinitionAttribute definition,
    bool isSupported,
    bool isFlag
  )
  {
    var names = definition.ArgumentName
      .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
      .Select(RemoveValueMarker)
      .ToArray();
    var optionType = isSupported ? property.PropertyType : typeof(string);
    var option = (CommandLineOption)
      Activator.CreateInstance(
        typeof(Option<>).MakeGenericType(optionType),
        ToOptionName(names[0]),
        names.Skip(1).Select(ToOptionName).ToArray()
      )!;
    option.Description = definition.Description;
    return option;
  }

  private static string[] NormalizeFlagArguments(
    string[] args,
    IReadOnlyList<PropertyBinding> bindings
  )
  {
    var flagNames = bindings
      .Where(x => x.IsFlag)
      .Select(x => x.Option.Name)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var normalized = new List<string>(args.Length);

    for (var i = 0; i < args.Length; i++)
    {
      var argument = args[i];
      normalized.Add(argument);
      if (
        flagNames.Contains(argument)
        && (i + 1 == args.Length || args[i + 1].StartsWith("-", StringComparison.Ordinal))
      )
      {
        normalized.Add(string.Empty);
      }
    }

    return [.. normalized];
  }

  private void ApplyValues(ParseResult parseResult, IReadOnlyList<PropertyBinding> bindings)
  {
    foreach (var binding in bindings)
    {
      var value = parseResult.GetValue<object>(binding.Option.Name);
      if (binding.IsCustom)
      {
        if (binding.IsFlag && parseResult.GetResult(binding.Option) is null)
        {
          continue;
        }

        var convertedValue = _customInterpreters[binding.Property.Name](value?.ToString() ?? string.Empty);
        if (convertedValue is not null)
        {
          binding.Property.SetValue(Args, convertedValue);
        }

        continue;
      }

      binding.Property.SetValue(Args, value);
    }
  }

  private void WriteHelp(RootCommand command)
  {
    Output.WriteLine("Options:");
    foreach (var property in GetProperties())
    {
      var definition = property.GetCustomAttribute<ArgumentDefinitionAttribute>()
        ?? throw new InvalidOperationException(
          "all command options require an attribute defining the argument"
        );
      Output.WriteLine($"  {definition.ArgumentName}: {definition.Description}");
    }
  }

  private static IEnumerable<PropertyInfo> GetProperties() =>
    typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);

  private static bool IsSupported(Type propertyType)
  {
    var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
    return type.IsEnum
      || type == typeof(string)
      || type == typeof(bool)
      || type == typeof(byte)
      || type == typeof(sbyte)
      || type == typeof(short)
      || type == typeof(ushort)
      || type == typeof(int)
      || type == typeof(uint)
      || type == typeof(long)
      || type == typeof(ulong)
      || type == typeof(float)
      || type == typeof(double)
      || type == typeof(decimal);
  }

  private static string RemoveValueMarker(string name) => name.TrimEnd('=', ':');

  private static string ToOptionName(string name) => name.Length == 1 ? $"-{name}" : $"--{name}";

  private sealed record PropertyBinding(
    PropertyInfo Property,
    CommandLineOption Option,
    bool IsCustom,
    bool IsFlag
  );
}

