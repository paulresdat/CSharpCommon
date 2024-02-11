using System.Diagnostics.CodeAnalysis;

namespace Csharp.Common.Utilities;

public interface IConsoleOutput
{
    IConsoleOutput WriteLine(params object[] message);
    IConsoleOutput Reset();
    IConsoleOutput SetColors(ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null);
    IConsoleOutput Write(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null);
    IConsoleOutput WriteLine(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null);
    IConsoleOutput WriteLine(object message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null);

    void SetCursorPosition(int left, int top);
    int CursorLeft { get; set; }
    int CursorTop { get; set; }
    ConsoleKeyInfo ReadKey();
    string? ReadLine();
}

// ConsoleOutput is a wrapper around Console with some color syntax niceties.  I don't think
// it needs to be covered by unit tests at this time.  Wrapping console.write to test
// is over doing it in my opinion.  This acts as both some nice sugar around .NET Console
// and color output as well as the capacity to mock for testing. 
[ExcludeFromCodeCoverage]
public class ConsoleOutput : IConsoleOutput
{
    private ConsoleColor CurrentForeground { get; set; }
    private ConsoleColor CurrentBackground { get; set; }

    public static ConsoleOutput Instance => new(); 

    public IConsoleOutput WriteLine(params object[] message)
    {
        foreach (var d in message)
        {
            WrapPrint(d, null, null, Console.WriteLine);
        }

        return this;
    }

    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public IConsoleOutput Reset()
    {
        Console.ResetColor();
        return this;
    }

    public IConsoleOutput SetColors(ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        if (foregroundColor != null)
        {
            CurrentForeground = (ConsoleColor)foregroundColor;
            Console.ForegroundColor = CurrentForeground;
        }

        if (backgroundColor != null)
        {
            CurrentBackground = (ConsoleColor) backgroundColor;
            Console.BackgroundColor = CurrentBackground;
        }

        return this;
    }

    public IConsoleOutput Write(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        WrapPrint(message, foregroundColor, backgroundColor, Console.Write);
        return this;
    }

    public IConsoleOutput WriteLine(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        WrapPrint(message, foregroundColor, backgroundColor, Console.WriteLine);
        return this;
    }

    public IConsoleOutput WriteLine(object message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        WrapPrint(message, foregroundColor, backgroundColor, Console.WriteLine);
        return this;
    }

    public void SetCursorPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
    }

    public int CursorLeft
    {
        get => Console.CursorLeft;
        set => Console.CursorLeft = value;
    }

    public int CursorTop
    {
        get => Console.CursorTop;
        set => Console.CursorTop = value;
    }

    public ConsoleKeyInfo ReadKey()
    {
        return Console.ReadKey();
    }

    private void WrapPrint(object data, ConsoleColor? foregroundColor, ConsoleColor? backgroundColor, Action<string?> callback)
    {
        var isColored = foregroundColor is not null || backgroundColor is not null;
        if (isColored)
        {
            SetColors(foregroundColor, backgroundColor);
        }
        callback(data.ToString());
        if (isColored)
        {
            Reset();
        }
    }
}
