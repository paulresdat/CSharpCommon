namespace Csharp.Common.Utilities.ConsoleCommander;

public class QuitOutOfAdminConsoleException : Exception
{
    public bool CustomMessageSet { get; set; }
    public QuitOutOfAdminConsoleException()
    {
        
    }

    public QuitOutOfAdminConsoleException(string message) : base(message)
    {
        CustomMessageSet = true;
    }
}
