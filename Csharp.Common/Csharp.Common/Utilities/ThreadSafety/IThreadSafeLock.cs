namespace Csharp.Common.Utilities.ThreadSafety;

public interface IThreadSafeLock
{
    void EnterReadLock(Action action);
    T EnterReadLock<T>(Func<T> action);
    void EnterReadWriteLock(Action action);
    T EnterReadWriteLock<T>(Func<T> action);

    void TryEnterReadLock(Action action, TimeSpan tryTimeSpan);
    T TryEnterReadLock<T>(Func<T> action, TimeSpan tryTimeSpan);
    void TryEnterReadWriteLock(Action action, TimeSpan tryTimeSpan);
    T TryEnterReadWriteLock<T>(Func<T> action, TimeSpan tryTimeSpan);

    void EnterUpgradeableReadLock(Action action);
    T EnterUpgradeableReadLock<T>(Func<T> action);
    void TryEnterUpgradeableReadLock(Action action, TimeSpan tryTimeSpan);
    T TryEnterUpgradeableReadLock<T>(Func<T> action, TimeSpan tryTimeSpan);

    bool InRead { get; }
    bool InWrite { get; }
    bool InReadOrWrite { get; }
}