namespace Csharp.Common.EntityFramework.Repositories;

public class RepositoryBaseException : Exception
{
    private string OverrideStackTrace { get; } = "";

    /// <summary>
    /// This is used primarily for unit tests
    /// </summary>
    public RepositoryBaseException() : base("For testing exception handling in unit/integration tests")
    {

    }

    /// <summary>
    /// The constructor that must be used when throwing exceptions in the BaseRepositoryLayer
    /// </summary>
    /// <param name="message"></param>
    /// <param name="exceptions"></param>
    public RepositoryBaseException(string message, List<Exception>? exceptions) : base(message)
    {
        if (exceptions != null)
        {
            foreach (var exc in exceptions)
            {
                OverrideStackTrace += exc.Message + "\n";
                OverrideStackTrace += exc.StackTrace;
                OverrideStackTrace += "\n\n";
            }
        }
    }

    public override string StackTrace
    {
        get
        {
            if (OverrideStackTrace != "")
            {
                return OverrideStackTrace;
            }

            return StackTrace;
        }
    }
}