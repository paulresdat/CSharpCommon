using System;

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

    private OptionSet? Options { get; set; }
    private Action? HelpHeader { get; set; }

    protected void SetOptions(OptionSet optionSet)
    {
        Options = optionSet;
        Options.Add("h|help", "Help", v => Help = v != null);
    }

    public void ParseArguments(string[] args)
    {
        try
        {
            Options?.Parse(args);
        }
        catch (OptionException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine("Invalid argument detected.");
            return;
        }

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
        Console.WriteLine("Options:");
        p?.WriteOptionDescriptions(Console.Out);
    }
}
