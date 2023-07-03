using System.Text.RegularExpressions;

namespace Csharp.Common.Utilities.ConsoleCommander;

public interface IConsoleCommandList
{
    IConsoleCommandList AddCommand(string command, string description, Action action, string? regex = null);
    IConsoleCommandList AddCommand(string command, string description, Action<string> action, string? regex = null);
    IConsoleCommandList AddCommand(string command, string description, Func<string, Task> action, string? regex = null);
    Task RunCommand(string command);
    List<string[]> Commands { get; }
}

public class NnCommandList : IConsoleCommandList
{
    private readonly List<NnCommand> _commands = new();

    public IConsoleCommandList AddCommand(string command, string description, Action action, string? regex = null)
    {
        var nnCommand = new NnCommand<Action<string>>
        {
            Command = command,
            RegexStr = (!string.IsNullOrWhiteSpace(regex) ? regex : command),
            Description = description,
            Action = (str) => action(),
        };

        _commands.Add(nnCommand);

        return this;
    }
    
    public IConsoleCommandList AddCommand(string command, string description, Action<string> action, string? regex = null)
    {
        var nnCommand = new NnCommand<Action<string>>
        {
            Command = command,
            RegexStr = (!string.IsNullOrWhiteSpace(regex) ? regex : command),
            Description = description,
            Action = action
        };

        _commands.Add(nnCommand);

        return this;
    }

    public IConsoleCommandList AddCommand(string command, string description, Func<string, Task> action, string? regex = null)
    {
        var nnCommand = new NnCommand<Func<string, Task>>
        {
            Command = command,
            RegexStr = (!string.IsNullOrWhiteSpace(regex) ? regex : command),
            Description = description,
            ActionAsync = action
        };

        _commands.Add(nnCommand);
        return this;
    }

    public async Task RunCommand(string command)
    {
        if (command == "")
        {
            return;
        }
        if (_commands.All(x => !Regex.IsMatch(command, "^" + x.RegexStr + "$")))
        {
            throw new NnCommandListException("The command: '" + command + "' does not exist as a command");
        }

        var cmd = _commands.First(x => Regex.IsMatch(command, "^" + x.RegexStr + "$"));
        if (cmd.IsAsync)
        {
            await ((Func<string, Task>)cmd.GetAction).Invoke(command);
        }
        else
        {
            ((Action<string>)cmd.GetAction).Invoke(command);
        }
    }

    public List<string[]> Commands
    {
        get
        {
            return _commands.Select(x => new string[]
                {
                    x.Command ?? "",
                    x.Description ?? "",
                })
                .ToList();
        }
    }
}
