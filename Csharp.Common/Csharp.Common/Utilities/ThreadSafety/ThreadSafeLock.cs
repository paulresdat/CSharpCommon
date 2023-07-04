namespace Csharp.Common.Utilities;

public class ThreadSafeLock : ThreadSafe, IThreadSafe
{
    public new bool InRead => base.InRead;
    public new bool InWrite => base.InWrite;
    public new bool InReadOrWrite => base.InReadOrWrite;

    public new void EnterReadLock(Action action)
    {
        base.EnterReadLock(action);
    }

    public new T EnterReadLock<T>(Func<T> action)
    {
        return base.EnterReadLock(action);
    }

    public new void EnterReadWriteLock(Action action)
    {
        base.EnterReadWriteLock(action);
    }

    public new T EnterReadWriteLock<T>(Func<T> action)
    {
        return base.EnterReadWriteLock(action);
    }
}