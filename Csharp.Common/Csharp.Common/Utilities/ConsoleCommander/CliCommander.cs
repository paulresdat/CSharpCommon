using Spectre.Console;

namespace Csharp.Common.Utilities.ConsoleCommander;

/// <summary>
/// The important interface that matters to the outside facing service.  This is the entry point of admin console
/// from the perspective of what's calling it.
/// </summary>
/// <example>
/// <code>
/// var adminConsole = new AdminConsoleTest();
/// // the only exposed method outside of the object that extends the abstract class
/// adminConsole.RunAdmin();
/// </code>
/// </example>
public interface ICli
{
    /// <summary>
    /// Run admin console!  The main entry point, the place where it starts!  When you run this command, admin console
    /// spins up and waits for commands.  Every command has a prompt, it tells you when the command isn't recognized
    /// and will run code specified by you for each command registered.  Quitting throws a quit exception which is caught
    /// and then prints the message out before exiting admin console functionality.
    /// </summary>
    void RunCli();
}

public interface ICommandListFluency
{
    ICommandListFluency AddCommand(string command, string description, Action action, string? regex = null);
    ICommandListFluency AddCommand(string command, string description, Action<string> action, string? regex = null);
    ICommandListFluency AddCommand(string command, string description, Func<Task> action, string? regex = null);
    ICommandListFluency AddCommand(string command, string description, Func<string, Task> action, string? regex = null);
}

/// <summary>
/// <para>Console Commander Abstract Class</para>
///
/// <para>
/// This abstract serves as the boiler plate of code that allows for a kind of command line system in a console application.
/// You type in a command and it will run that command essentially.  If you have a console app that runs as a service,
/// you can use this to extend your upper service layer class to facilitate an administrative command line to introspect
/// into the application without as much log analysis.
/// </para>
///
/// <para>
/// An extra benefit to this functionality is the ability to troubleshoot while the application is running.  If you break
/// up your code in a well defined interface (using something like ILinuxService), you can break apart the critical
/// components of the application and monitor in real time as the application runs, inspecting critical data structures
/// at a specific moment, or trace actions as the application handles events.
/// </para>
///
/// <para>
/// To start, you must first extend a top layer service application you call centrally, or to create a dedicated
/// admin console class that has many of the internal singletons injected into that classes constructor.  You then
/// create commands around the functionality you injected.  For instance, at the service layer, you may have a
/// "StartMonitoring" method that starts up the monitor of an ITCM or RabbitMQ queue.  You would make a new console
/// command that can start and stop that monitor when the command is given on the command line.
/// </para>
/// </summary>
/// 
/// <example>
/// <code>
/// public class TestConsole : AdminConsole
/// {
///     public TestConsole(
///         IConsoleCommander consoleCommander,
///         IConsoleCommandList commandList) : 
///         base(consoleCommander, commandList)
///     {
///         // custom command line sigil
///         ConsoleCommander.SetConsolePrompt("[enter-command]>$ ");
///     }
///
///     protected override void InitializeAdminCommands(ICommandListFluency commandList)
///     {
///         var cnsl = Cnsl.Instance;
///         commandList
///             .AddCommand("hello-world", "Hello world", () =>
///             {
///                 // hello world is printed out every time hello-world is furnished
///                 cnsl.WriteLine("Hello world");
///             })
///             .AddCommand("print [id-using-regex]", "Print what's given", regex: @"print\s+(\d+)", action: async (s) =>
///             {
///                 // a regular expression example of allowing for input to be specified with the command
///                 var tmp = Regex.Replace(s.Trim(), @"\s{2,}", "").Split(" ");
///                 cnsl.WriteLine("Printing: " + string.Join(" ", tmp[1..]));
///             });
///     }
///
///     protected override void AdminSplashScreen(Cnsl cnsl)
///     {
///         cnsl.SetColors(ConsoleColor.Blue).WriteLine("Admin console testing!").Reset();
///     }
/// }
/// </code>
/// </example>
public abstract class CliCommander : ICommandListFluency, ICli
{
    private readonly IConsoleCommandList _commandList;
    private readonly ICommandLineProcessor _commandLineProcessor;
    private readonly IConsoleOutput _consoleOutput;
    protected IConsoleCommandList Commands => _commandList;
    protected ICommandLineProcessor CommandLineProcessor => _commandLineProcessor;

    protected CliCommander(
        ICommandLineProcessor commandLineProcessor,
        IConsoleCommandList commandList,
        IConsoleOutput consoleOutput)
    {
        _commandLineProcessor = commandLineProcessor;
        _commandList = commandList;
        _consoleOutput = consoleOutput;
    }

    /// <summary>
    /// This is where you register your admin console commands.
    /// </summary>
    /// <param name="commandList"></param>
    protected abstract void InitializeAdminCommands(ICommandListFluency commandList);

    /// <summary>
    /// This is the splash screen that is called every time admin console starts.
    /// </summary>
    /// <param name="consoleOutput"></param>
    protected abstract void AdminSplashScreen(IConsoleOutput consoleOutput);

    /// <summary>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="description"></param>
    /// <param name="action"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public ICommandListFluency AddCommand(string command, string description, Action action, string? regex = null)
    {
        Commands.AddCommand(command, description, action, regex);
        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="description"></param>
    /// <param name="action"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public ICommandListFluency AddCommand(string command, string description, Action<string> action, string? regex = null)
    {
        Commands.AddCommand(command, description, action, regex);
        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="description"></param>
    /// <param name="action"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public ICommandListFluency AddCommand(string command, string description, Func<Task> action, string? regex = null)
    {
        Commands.AddCommand(command, description, action, regex);
        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="command"></param>
    /// <param name="description"></param>
    /// <param name="action"></param>
    /// <param name="regex"></param>
    /// <returns></returns>
    public ICommandListFluency AddCommand(string command, string description, Func<string, Task> action, string? regex = null)
    {
        Commands.AddCommand(command, description, action, regex);
        return this;
    }

    public void RunCli()
    {
        InitializeAdminCommands(this);
        AddCommand("?", "Show Menu", (s) => PrintMenu());
        AdminSplashScreen(_consoleOutput);
        PrintMenu();
        ReadFromInput();
    }
    
    protected void AskForYesOrNo(string line, Action onYes, Action? onNo = null, string defaultValue = "n")
    {
        AskForInput(line + " [Y/n] ", s =>
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                s = defaultValue;
            }

            if (s == "Y" || s == "n")
            {
                switch (s)
                {
                    case "Y": onYes();
                        break;
                    case "n": onNo?.Invoke();
                        break;
                }
                return true;
            }

            return false;
        });
    }

    protected void AskForInput(string line, Func<string, bool> action)
    {
        bool loop;
        do
        {
            _consoleOutput.Write(line);
            var newLine = Console.ReadLine()?.Trim() ?? "";
            loop = !action(newLine);
        } while (loop);
    }

    private void PrintMenu()
    {
        var table = new Table();
        table.AddColumns("[yellow]Command[/]", "[yellow]Description[/]");
        foreach (var command in Commands.Commands)
        {
            table.AddRow("[green]" + command[0] + "[/]", command[1]);
        }

        table.Border(TableBorder.Rounded);
        AnsiConsole.Write(table);
    }

    private async void ReadFromInput(Action<IConsoleCommandList>? onQuit = null)
    {
        var quit = false;
        while (!quit)
        {
            try
            {
                var data = CommandLineProcessor.ReadLine();
                await Commands.RunCommandAsync(data.Trim());
            }
            catch (CommandListException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (QuitOutOfConsoleCommanderException e)
            {
                // do stuff!
                Console.WriteLine(
                    !e.CustomMessageSet
                        ? "Quitting, shutting stuff down.  If this doesn't automatically stop, there is an issue"
                        : e.Message);
                quit = true;
                onQuit?.Invoke(Commands);
            }
            catch (Exception e)
            {
                Console.WriteLine("A general exception was thrown when running the previous command: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}