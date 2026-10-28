namespace Csharp.Common.Utilities.ThreadSafety;

public class ThreadSafeLockingFactoryException : Exception
{
  public ThreadSafeLockingFactoryException(string message)
    : base(message) { }
}
