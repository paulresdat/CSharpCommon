namespace Csharp.Common.Utilities.ThreadSafety;

public interface IThreadSafeLockingFactory
{
    void EnterReadLock<T>(T lockKey, Action action) where T : notnull;
    T2 EnterReadLock<T, T2>(T lockKey, Func<T2> action) where T : notnull;
    void EnterReadWriteLock<T>(T lockKey, Action action) where T : notnull;
    T2 EnterReadWriteLock<T, T2>(T lockKey, Func<T2> action) where T : notnull;

    void EnterUpgradeableReadLock<T>(T lockKey, Action action) where T : notnull;
    T2 EnterUpgradeableReadLock<T, T2>(T lockKey, Func<T2> action) where T : notnull;
    void TryEnterUpgradeableReadLock<T>(T lockKey, Action action, TimeSpan timeSpan) where T : notnull;
    T2 TryEnterUpgradeableReadLock<T, T2>(T lockKey, Func<T2> action, TimeSpan timeSpan) where T : notnull;
    void TryEnterUpgradeableReadLock<T>(T lockKey, Action action, int millisecondsTimeout) where T : notnull;
    T2 TryEnterUpgradeableReadLock<T, T2>(T lockKey, Func<T2> action, int millisecondsTimeout) where T : notnull;

    void TryEnterReadLock<T>(T lockKey, Action action, TimeSpan tryTimeSpan) where T : notnull;
    T2 TryEnterReadLock<T, T2>(T lockKey, Func<T2> action, TimeSpan tryTimeSpan) where T : notnull;
    void TryEnterReadWriteLock<T>(T lockKey, Action action, TimeSpan tryTimeSpan) where T : notnull;
    T2 TryEnterReadWriteLock<T, T2>(T lockKey, Func<T2> action, TimeSpan tryTimeSpan) where T : notnull;
    void TryEnterReadLock<T>(T lockKey, Action action, int millisecondsTimeout) where T : notnull;
    T2 TryEnterReadLock<T, T2>(T lockKey, Func<T2> action, int millisecondsTimeout) where T : notnull;
    void TryEnterReadWriteLock<T>(T lockKey, Action action, int millisecondsTimeout) where T : notnull;
    T2 TryEnterReadWriteLock<T, T2>(T lockKey, Func<T2> action, int millisecondsTimeout) where T : notnull;
}

/// <summary>
/// <para>
/// This class offers a granular approach to interlocking on a complex data structure
/// by a key, rather than locking on the data structure itself.
/// </para>
///
/// <para>
/// A good example would be a list of references to class objects.  The class objects get
/// updated but not the list itself and you have multiple threads that could be be updating
/// multiple objects at once.  Instead of locking on the entire list, you want to lock on
/// the object itself.  Well, that's also not good practice since we know that locking on the
/// object is not recommended nor a best practice.  Instead we have to generate a lock based
/// on the key identifier of the object in the list (hash code or unique string identifier).
/// This factory instead allows you to do just that.  By using the locking factory, you can
/// enter a read lock on the list and then a write lock on the object key identifier and
/// operate on the object.  As long as the list doesn't change and is only read from, this
/// is thread safe.
/// </para>
///
/// <para>
/// I would suggest that if you are locking on a single object, that instead of using the
/// thread lock factory, you inherit `ThreadSafe`.  It's idiomatic to encapsulate CRUD
/// operations on a thread sensitive object by encapsulating it as a readonly private property
/// of the parent class and expose CRUD methods that enter lock states in the parent class.
/// Please see <see cref="ThreadSafe">ThreadSafe</see> for more details on its use.
/// </para>
///
/// <para>
/// Caveat: this is not safe on Dictionaries.  Before operating on complex data structures
/// it would be wise to consult documentation on the inner workings of memory safe data types in
/// C# before locking on it.  Locking on Dictionaries by key may not be inherently thread safe since
/// internally you're accessing linked lists by hashcode key so a dictionary key is not truly
/// unique.  Since keys with the same hashcode get stored in the same list, you may want to lock on
/// the generated hashcode value of the key rather than key itself in the dictionary but even then,
/// that might not be safe because under the hood, the data structure may change the positions of
/// hashed keys in the linked list for performance reasons.  Check the docs before you try and lock
/// on parts of a complex built-in data structure.
/// </para>
///
/// <para>
/// Because of this feature and inherent design of dictionary hashcode lookup, the thread safety
/// factory should probably not be used for very large data sets that require locking on string
/// keys.
/// </para>
/// </summary>
public class ThreadSafeLockingFactory: ThreadSafe, IThreadSafeLockingFactory
{
    private readonly Dictionary<object, IThreadSafeLock> _locks = new();

    public void EnterReadLock<T>(T lockKey, Action action) where T: notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.EnterReadLock(action);
    }

    public T2 EnterReadLock<T, T2>(T lockKey, Func<T2> action) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.EnterReadLock(action);
    }

    public void EnterReadWriteLock<T>(T lockKey, Action action) where T: notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.EnterReadWriteLock(action);
    }

    public T2 EnterReadWriteLock<T, T2>(T lockKey, Func<T2> action) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.EnterReadWriteLock(action);
    }

    public void EnterUpgradeableReadLock<T>(T lockKey, Action action) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.EnterUpgradeableReadLock(action);
    }

    public T2 EnterUpgradeableReadLock<T, T2>(T lockKey, Func<T2> action) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.EnterUpgradeableReadLock(action);
    }

    public void TryEnterUpgradeableReadLock<T>(T lockKey, Action action, TimeSpan timeSpan) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.TryEnterUpgradeableReadLock(action, timeSpan);
    }

    public T2 TryEnterUpgradeableReadLock<T, T2>(T lockKey, Func<T2> action, TimeSpan timeSpan) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.TryEnterUpgradeableReadLock(action, timeSpan);
    }

    public void TryEnterUpgradeableReadLock<T>(T lockKey, Action action, int millisecondsTimeout) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.TryEnterUpgradeableReadLock(action, millisecondsTimeout);
    }

    public T2 TryEnterUpgradeableReadLock<T, T2>(T lockKey, Func<T2> action, int millisecondsTimeout) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.TryEnterUpgradeableReadLock(action, millisecondsTimeout);
    }

    public void TryEnterReadLock<T>(T lockKey, Action action, TimeSpan tryTimeSpan) where T: notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.TryEnterReadLock(action, tryTimeSpan);
    }

    public T2 TryEnterReadLock<T, T2>(T lockKey, Func<T2> action, TimeSpan tryTimeSpan) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.TryEnterReadLock(action, tryTimeSpan);
    }

    public void TryEnterReadWriteLock<T>(T lockKey, Action action, TimeSpan tryTimeSpan) where T: notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.TryEnterReadWriteLock(action, tryTimeSpan);
    }

    public T2 TryEnterReadWriteLock<T, T2>(T lockKey, Func<T2> action, TimeSpan tryTimeSpan) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.TryEnterReadWriteLock(action, tryTimeSpan);
    }

    public void TryEnterReadLock<T>(T lockKey, Action action, int millisecondsTimeout) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.TryEnterReadLock(action, millisecondsTimeout);
    }

    public T2 TryEnterReadLock<T, T2>(T lockKey, Func<T2> action, int millisecondsTimeout) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.TryEnterReadLock(action, millisecondsTimeout);
    }

    public void TryEnterReadWriteLock<T>(T lockKey, Action action, int millisecondsTimeout) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        lockObject.TryEnterReadWriteLock(action, millisecondsTimeout);
    }

    public T2 TryEnterReadWriteLock<T, T2>(T lockKey, Func<T2> action, int millisecondsTimeout) where T : notnull
    {
        var lockObject = TryCreateLock(lockKey);
        return lockObject.TryEnterReadWriteLock(action, millisecondsTimeout);
    }

    private IThreadSafeLock TryCreateLock(object lockKey)
    {
        var @lock = EnterReadLock(() =>
        {
            if (_locks.TryGetValue(lockKey, out var lock1))
            {
                return lock1;
            }

            return null;
        });

        if (@lock is null)
        {
            // for some safety on generating locks, we'll want to lock on creating a key for the first time
            return EnterReadWriteLock(() =>
            {
                _locks[lockKey] = new ThreadSafeLock();
                return _locks[lockKey];
            });
        }

        return @lock;
    }
}