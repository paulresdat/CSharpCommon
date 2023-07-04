namespace Csharp.Common.Utilities.ThreadSafety;

public interface IThreadSafe
{
    void EnterReadLock(Action action);
    T EnterReadLock<T>(Func<T> action);
    void EnterReadWriteLock(Action action);
    T EnterReadWriteLock<T>(Func<T> action);
    bool InRead { get; }
    bool InWrite { get; }
    bool InReadOrWrite { get; }
}