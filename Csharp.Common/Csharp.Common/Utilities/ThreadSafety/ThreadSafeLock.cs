namespace Csharp.Common.Utilities.ThreadSafety;

public class ThreadSafeLock : ThreadSafe, IThreadSafeLock
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
    
    public new void TryEnterReadLock(Action action, TimeSpan tryTimeSpan)
    {
        base.TryEnterReadLock(action, tryTimeSpan);
    }

    public new T TryEnterReadLock<T>(Func<T> action, TimeSpan tryTimeSpan)
    {
        return base.TryEnterReadLock(action, tryTimeSpan);
    }

    public new void TryEnterReadWriteLock(Action action, TimeSpan tryTimeSpan)
    {
        base.TryEnterReadWriteLock(action, tryTimeSpan);
    }

    public new T TryEnterReadWriteLock<T>(Func<T> action, TimeSpan tryTimeSpan)
    {
        return base.TryEnterReadWriteLock(action, tryTimeSpan);
    }

    public new void EnterUpgradeableReadLock(Action action)
    {
        base.EnterUpgradeableReadLock(action);
    }

    public new T EnterUpgradeableReadLock<T>(Func<T> action)
    {
        return base.EnterUpgradeableReadLock(action);
    }

    public new void TryEnterUpgradeableReadLock(Action action, TimeSpan tryTimeSpan)
    {
        base.TryEnterUpgradeableReadLock(action, tryTimeSpan);
    }
    
    public new T TryEnterUpgradeableReadLock<T>(Func<T> action, TimeSpan tryTimeSpan)
    {
        return base.TryEnterUpgradeableReadLock(action, tryTimeSpan);
    }
}