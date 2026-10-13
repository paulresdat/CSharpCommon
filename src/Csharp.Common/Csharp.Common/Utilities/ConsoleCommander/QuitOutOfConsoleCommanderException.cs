namespace Csharp.Common.Utilities.ConsoleCommander;

public class QuitOutOfConsoleCommanderException : Exception
{
    public bool CustomMessageSet { get; set; }
    public QuitOutOfConsoleCommanderException()
    {
        
    }

    public QuitOutOfConsoleCommanderException(string message) : base(message)
    {
        CustomMessageSet = true;
    }
}
