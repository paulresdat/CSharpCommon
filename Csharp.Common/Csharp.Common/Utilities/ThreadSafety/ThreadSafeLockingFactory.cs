namespace Csharp.Common.Utilities.ThreadSafety;

public interface IThreadSafeLockingFactory
{
    void EnterReadLock<T>(T lockKey, Action action) where T : notnull;
    T2 EnterReadLock<T, T2>(T lockKey, Func<T2> action) where T : notnull;
    void EnterReadWriteLock<T>(T lockKey, Action action) where T : notnull;
    T2 EnterReadWriteLock<T, T2>(T lockKey, Func<T2> action) where T : notnull;

}

/// <summary>
/// This class offers a granular approach to interlocking on a complex data structure
/// by a key, rather than locking on the data structure itself.
///
/// A good example would be a list of references to class objects.  The class objects get
/// updated but not the list itself and you have multiple threads that could be be updating
/// multiple objects at once.  Instead of locking on the entire list, you want to lock on
/// the object itself.  Well, that's also not good practice since we know that locking on the
/// object is not recommended nor a best practice.  Instead we have to generate a lock based
/// on the key identifier of the object in the list (hash code or unique string identifier).
/// This factory instead allows you to do just that.  By using the locking factory, you can
/// enter a read state on the list and then a write state on the object key identifier and
/// operate on the object.  As long as the list doesn't change and is only read from, this
/// is thread safe.
/// 
/// Caveat: this is not safe on Dictionaries.  Before operating on complex data structures
/// it would be wise to consult documentation on the inner workings of "safe" data types in
/// C# before locking on the type.  Locking on Dictionaries by key may not be inherently thread safe since
/// internally you're accessing linked lists by hashcode key and can change the linked list
/// that has the same hashcode but different keys.  You may want to lock on the generated hashcode
/// value of the object rather than the string key in the dictionary but even then, that might
/// not be safe because under the hood, the data structure may change the positions of hashed keys
/// in the linked list for performance reasons.  Check the docs before you try and lock on parts
/// of a complex built-in data structure.
/// </summary>
public class ThreadSafeLockingFactory: IThreadSafeLockingFactory
{
    private readonly Dictionary<object, IThreadSafe> _locks = new();

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

    private IThreadSafe TryCreateLock(object lockKey)
    {
        if (_locks.ContainsKey(lockKey))
        {
            return _locks[lockKey];
        }

        _locks[lockKey] = new ThreadSafeLock();
        return _locks[lockKey];
    }
}