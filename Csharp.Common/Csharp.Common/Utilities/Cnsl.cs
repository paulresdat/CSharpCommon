using System;

namespace Csharp.Common.Utilities;

public class Cnsl
{
    private ConsoleColor CurrentForeground { get; set; }
    private ConsoleColor CurrentBackground { get; set; }

    public static Cnsl Instance => new Cnsl(); 

    public Cnsl Reset()
    {
        Console.ResetColor();
        return this;
    }

    public Cnsl SetColors(ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
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

    public Cnsl Write(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        WrapPrint(message, foregroundColor, backgroundColor, Console.Write);
        return this;
    }

    public Cnsl WriteLine(string message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        WrapPrint(message, foregroundColor, backgroundColor, Console.WriteLine);
        return this;
    }

    public Cnsl WriteLine(object message, ConsoleColor? foregroundColor = null, ConsoleColor? backgroundColor = null)
    {
        WrapPrint(message, foregroundColor, backgroundColor, Console.WriteLine);
        return this;
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

    public Cnsl WriteLine(params object[] message)
    {
        foreach (var d in message)
        {
            WrapPrint(d, null, null, Console.WriteLine);
        }

        return this;
    }
}
