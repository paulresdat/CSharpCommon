using Csharp.Common.Utilities;
using Csharp.Common.Utilities.ConsoleCommander;
using Csharp.Common.Utilities.WaitFor;

namespace ConsoleCommander;

public class BasicCli : CliCommander
{
    public BasicCli(
        ICommandLineProcessor commandLineProcessor,
        IConsoleCommandList commandList,
        IConsoleOutput consoleOutput) : base(commandLineProcessor, commandList, consoleOutput)
    {
    }

    protected override void InitializeAdminCommands(ICommandListFluency commandList)
    {
        commandList
            .AddCommand("test", "Testing the comand", () =>
            {
                Out.WriteLine("Testing, testing! 123!  Can you read me!");
            })
            .AddCommand("test (args)", "Testing regex", regex: @"test\s+(\w+)", action: (s) =>
            {
                Out.WriteLine("Regex passed: " + s);
            })
            .AddCommand("x|quit|exit|q", "Quit", regex: @"x|quit|exit|q", action: () =>
            {
                throw new QuitOutOfConsoleCommanderException("");
            });
    }

    protected override void AdminSplashScreen(IConsoleOutput consoleOutput)
    {
        var s = @"
░░      ░░░░      ░░░   ░░░  ░░░      ░░░░      ░░░  ░░░░░░░░        
▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒    ▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒▒▒
▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓  ▓  ▓  ▓▓▓      ▓▓▓  ▓▓▓▓  ▓▓  ▓▓▓▓▓▓▓▓      ▓▓
█  ████  ██  ████  ██  ██    ████████  ██  ████  ██  ████████  ██████
██      ████      ███  ███   ███      ████      ███        ██        
░░░      ░░░░      ░░░  ░░░░  ░░  ░░░░  ░░░      ░░░   ░░░  ░░       ░░░        ░░       ░░
▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒  ▒▒   ▒▒   ▒▒   ▒▒   ▒▒  ▒▒▒▒  ▒▒    ▒▒  ▒▒  ▒▒▒▒  ▒▒  ▒▒▒▒▒▒▒▒  ▒▒▒▒  ▒
▓▓  ▓▓▓▓▓▓▓▓  ▓▓▓▓  ▓▓        ▓▓        ▓▓  ▓▓▓▓  ▓▓  ▓  ▓  ▓▓  ▓▓▓▓  ▓▓      ▓▓▓▓       ▓▓
██  ████  ██  ████  ██  █  █  ██  █  █  ██        ██  ██    ██  ████  ██  ████████  ███  ██
███      ████      ███  ████  ██  ████  ██  ████  ██  ███   ██       ███        ██  ████  █
";
        Out.WriteLine(s, ConsoleColor.Magenta);
    }
}