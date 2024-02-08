using Microsoft.Extensions.Logging;

namespace Csharp.Common.Utilities.ThreadSafety;

/// <summary>
/// <para>Thread Safety abstract class that allows the ability for us to manage locks for both read and writes.  We use this
/// class to wrap a data structure managing class to be thread safe.</para>
///
/// <para>
/// Utilizing this class adheres to the approach that locked structures are important and have a set of methods around them
/// to update the data of these structures.  What this implies is a separation of concern around that locking mechanism
/// and that every data structure that requires locks is encapsulated in its own class with its own methods that operate
/// on that data structure.  This threading mechanism works best under this paradigm.
/// </para>
/// 
/// <para>
/// If you have several different kinds of lock structures within a single entity, this class is probably not for you.
/// If you want to use this functionality for your locking but have the above problem, then you'll want to refactor your
/// code to adhere to a separation of concern on the locking mechanisms you have jumbled up into a single class.  Below
/// describes how you can refactor your code to work better with this class.
/// </para>
///
/// <para>
/// HOW TO APPROACH A REFACTOR<br/><br/>
///
/// Each data structure you have has a lock, if combined with several locks in one class, then it's probably code stink.
/// Meaning you haven't separated your important structures as dedicated objects that are manipulated upon from an
/// outside algorithm.  If you have multiple thread safe objects in one class, then you probably haven't separated your
/// concerns far enough and will need to rip out the code that operates on those data structures into new classes for each
/// critical data structure.  You'll expose the necessary CRUD operations like a repository in an interface for these
/// critical data structures. Then you'll want to extend each class with <c>ThreadSafe</c>.  After extending, you no
/// longer need to manage any locking objects, try catches, or any other fail safe mechanisms around locking.  Within a
/// public facing method that operates on the data structure, you wrap your critical code in a read or read/write lock.
/// And that's it.
/// </para>
///
/// <para>
/// <example>
/// An example of calling a lock:
/// <code>
/// public void UpdateData(string data)
/// {
///   EnterReadWriteLock(() => {
///     _myDataStructure.Data = data;
///   });
/// }
/// </code>
/// </example>
///
/// /// <example>
/// An example of using ThreadSafe:
/// <code>
/// public class MyClass : ThreadSafe
/// {
///    private readonly List{string} _myThreadSensitiveData = new();
///    public MyClass( ... ) { }
///
///    public string Read(int num)
///    {
///         return EnterReadLock(() => {
///             return _myThreadSensitiveData[num];
///         });
///    }
///
///    public void Write(int num, string data)
///    {
///         EnterReadWriteLock(() => {
///             _myThreadSensitiveData[num] = data;
///         });
///    }
/// 
///    ....
/// }
/// </code>
/// </example>
/// </para>
/// </summary>
public abstract class ThreadSafe
{
    private readonly ReaderWriterLockSlim _lockSlim = new();
    protected ILogger? Logger { get; set; }
    private const string LockRecursionText = 
        "A read lock recursion exception has been thrown while writing and is a critical error for the service to continue running, shutting down";

    // /// <summary>
    // /// Can be an extremely important element to check for.  Returns whether the object is on read a lock.
    // /// </summary>
    // protected bool InRead => _lockSlim.IsReadLockHeld;
    // /// <summary>
    // /// Can be an extremely important element to check for.  Returns whether the object is on read/write a lock.
    // /// </summary>
    // protected bool InWrite => _lockSlim.IsWriteLockHeld;
    // protected bool InReadOrWrite => InRead || InWrite;

    /// <summary>
    /// Allows for a void read, in case you're manipulating data outside the scope of the read lock
    /// or some other kind of function that doesn't require a write lock on the object being locked
    /// </summary>
    /// <param name="action"></param>
    protected void EnterReadLock(Action action)
    {
        TryEnterLock(LockType.Read, action);
    }

    /// <summary>
    /// Expects an action that returns an object or type.  Enters a read lock and then the method supplied to should
    /// be acted upon in a thread safe way
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T EnterReadLock<T>(Func<T> action)
    {
        return TryEnterLock(LockType.Read, action);
    }

    /// <summary>
    /// Enter a read write lock without an expected return value
    /// </summary>
    /// <param name="action"></param>
    protected void EnterReadWriteLock(Action action)
    {
        TryEnterLock(LockType.Write, action);
    }
    
    /// <summary>
    /// Enter a write lock on the object and return the type defined by the function passed
    /// <br/>
    /// <example>
    /// <code>
    /// return EnterReadWriteLock(() => {
    ///   return 1;
    /// });
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T EnterReadWriteLock<T>(Func<T> action)
    {
        return TryEnterLock(LockType.Write, action);
    }

    /// <summary>
    /// Try to enter a read lock with a timespan timeout
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tryTimeSpan"></param>
    protected void TryEnterReadLock(Action action, TimeSpan tryTimeSpan)
    {
        TryEnterLock(LockType.TryRead, action, tryTimeSpan);
    }

    /// <summary>
    /// Try to enter a read lock with a int millisecond timeout
    /// </summary>
    /// <param name="action"></param>
    /// <param name="millisecondsTimeout"></param>
    protected void TryEnterReadLock(Action action, int millisecondsTimeout)
    {
        TryEnterLock(LockType.TryRead, action, null, millisecondsTimeout);
    }

    /// <summary>
    /// Try to enter a read lock with a timespan timeout with expected output from the passed action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tryTimeSpan"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T TryEnterReadLock<T>(Func<T> action, TimeSpan tryTimeSpan)
    {
        return TryEnterLock(LockType.TryRead, action, tryTimeSpan);
    }

    /// <summary>
    /// Try to enter a read lock with an integer millisecond timeout with expected output from the passed action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="millisecondsTimeout"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T TryEnterReadLock<T>(Func<T> action, int millisecondsTimeout)
    {
        return TryEnterLock(LockType.TryRead, action, null, millisecondsTimeout);
    }

    /// <summary>
    /// Try to enter a read write lock with a timespan
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tryTimeSpan"></param>
    protected void TryEnterReadWriteLock(Action action, TimeSpan tryTimeSpan)
    {
        TryEnterLock(LockType.TryWrite, action, tryTimeSpan);
    }

    /// <summary>
    /// Try to enter a read write lock with an integer millisecond timeout
    /// </summary>
    /// <param name="action"></param>
    /// <param name="millisecondsTimeout"></param>
    protected void TryEnterReadWriteLock(Action action, int millisecondsTimeout)
    {
        TryEnterLock(LockType.TryWrite, action, null, millisecondsTimeout);
    }

    /// <summary>
    /// Try to enter a read write lock with a timespan and a return value from the passed action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tryTimeSpan"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T TryEnterReadWriteLock<T>(Func<T> action, TimeSpan tryTimeSpan)
    {
        return TryEnterLock(LockType.TryWrite, action, tryTimeSpan);
    }

    /// <summary>
    /// Try to enter a read write lock with a timespan and a return value from the passed action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="millisecondTimeout"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T TryEnterReadWriteLock<T>(Func<T> action, int millisecondTimeout)
    {
        return TryEnterLock(LockType.TryWrite, action, null, millisecondTimeout);
    }

    /// <summary>
    /// Enter an upgradeable read lock, which allows you to run a write lock within the read lock block action
    ///
    /// <example>
    /// <code>
    /// EnterUpgradeableReadLock(() => {
    ///   EnterReadWriteLock(() => { });
    /// });
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="action"></param>
    protected void EnterUpgradeableReadLock(Action action)
    {
        TryEnterLock(LockType.UpgradeableRead, action);
    }

    /// <summary>
    /// Enter an upgradeable read lock, which allows you to run a write lock within the read lock block action
    ///
    /// <example>
    /// <code>
    /// return EnterUpgradeableReadLock(() => {
    ///   return EnterReadWriteLock(() => {
    ///     return 1;
    ///   });
    /// });
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T EnterUpgradeableReadLock<T>(Func<T> action)
    {
        return TryEnterLock(LockType.UpgradeableRead, action);
    }

    /// <summary>
    /// Try to enter an upgradeable lock with a timespan timeout
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tryTimeSpan"></param>
    protected void TryEnterUpgradeableReadLock(Action action, TimeSpan tryTimeSpan)
    {
        TryEnterLock(LockType.TryUpgradeableRead, action, tryTimeSpan);
    }

    /// <summary>
    /// Try to enter an upgradeable lock with a timespan timeout
    /// </summary>
    /// <param name="action"></param>
    /// <param name="millisecondsTimeout"></param>
    protected void TryEnterUpgradeableReadLock(Action action, int millisecondsTimeout)
    {
        TryEnterLock(LockType.TryUpgradeableRead, action, null, millisecondsTimeout);
    }

    /// <summary>
    /// Try to enter an upgradeable lock with a timespan timeout and a returning value from the passed action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="tryTimeSpan"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T TryEnterUpgradeableReadLock<T>(Func<T> action, TimeSpan tryTimeSpan)
    {
        return TryEnterLock(LockType.TryUpgradeableRead, action, tryTimeSpan);
    }

    /// <summary>
    /// Try to enter an upgradeable lock with a integer timeout and a returning value from the passed action
    /// </summary>
    /// <param name="action"></param>
    /// <param name="millisecondsTimeout"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T TryEnterUpgradeableReadLock<T>(Func<T> action, int millisecondsTimeout)
    {
        return TryEnterLock(LockType.TryUpgradeableRead, action, null, millisecondsTimeout);
    }

    #region private functions that take out the boiler plate of the try catches
    private void EnterLockByType(LockType lockType, TimeSpan? tryEnterLockTimeSpan = null, int? millisecondsTimeout = null)
    {
        switch (lockType)
        {
            case LockType.Read:
                _lockSlim.EnterReadLock();
                break;
            case LockType.Write:
                _lockSlim.EnterWriteLock();
                break;
            case LockType.UpgradeableRead:
                _lockSlim.EnterUpgradeableReadLock();
                break;
            case LockType.TryUpgradeableRead:
                if (millisecondsTimeout is not null)
                {
                    _lockSlim.TryEnterUpgradeableReadLock(
                        millisecondsTimeout ?? throw new InvalidOperationException(
                            "Try upgradable locks requires an integer or timespan value for the timeout limit"));
                }
                else
                {
                    _lockSlim.TryEnterUpgradeableReadLock(
                        tryEnterLockTimeSpan ?? throw new InvalidOperationException(
                            "Try upgradable locks requires an integer or timespan value for the timeout limit"));
                }
                break;
            case LockType.TryRead:
                if (millisecondsTimeout is not null)
                {
                    _lockSlim.TryEnterReadLock(
                        millisecondsTimeout ?? throw new InvalidOperationException(
                            "Try read locks requires an integer or timespan value for the timeout limit"));
                }
                else
                {
                    _lockSlim.TryEnterReadLock(
                        tryEnterLockTimeSpan ?? throw new InvalidOperationException(
                            "Try read locks requires an integer or timespan value for the timeout limit"));
                }
                break;
            case LockType.TryWrite:
                if (millisecondsTimeout is not null)
                {
                    _lockSlim.TryEnterWriteLock(
                        millisecondsTimeout ?? throw new InvalidOperationException(
                            "Try write locks require an integer or timespan value for the timeout limit"));
                }
                else
                {
                    _lockSlim.TryEnterWriteLock(
                        tryEnterLockTimeSpan ?? throw new InvalidOperationException(
                            "Try write locks require an integer or timespan value for the timeout limit"));
                }
                break;
            default:
                throw new InvalidOperationException("Unknown lock type: " + lockType);
        }
    }

    private void ExitLockByType(LockType lockType)
    {
        switch (lockType)
        {
            case LockType.Read:
            case LockType.TryRead:
                _lockSlim.ExitReadLock();
                break;
            case LockType.Write:
            case LockType.TryWrite:
                _lockSlim.ExitWriteLock();
                break;
            case LockType.UpgradeableRead:
            case LockType.TryUpgradeableRead:
                _lockSlim.ExitUpgradeableReadLock();
                break;
            default:
                throw new InvalidOperationException("Unknown lock type: " + lockType);
        }
    }

    private void TryEnterLock(LockType lockType, Action action, TimeSpan? tryEnterLockTimeSpan = null, int? millisecondsTimeout = null)
    {
        try
        {
            EnterLockByType(lockType, tryEnterLockTimeSpan, millisecondsTimeout);

            try
            {
                action();
            }
            finally
            {
                ExitLockByType(lockType);
            }
        }
        catch (LockRecursionException e)
        {
            Logger?.LogCritical(LockRecursionText);
            Logger?.LogCritical("Exception and stacktrace: {Exception}, {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
        catch (Exception e)
        {
            Logger?.LogCritical("An exception has occurred: {Exception}, {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
    }

    private T TryEnterLock<T>(LockType lockType, Func<T> action, TimeSpan? tryEnterLockTimeSpan = null, int? millisecondsTimeout = null)
    {
        try
        {
            EnterLockByType(lockType, tryEnterLockTimeSpan, millisecondsTimeout);

            try
            {
                return action();
            }
            finally
            {
                ExitLockByType(lockType);
            }
        }
        catch (LockRecursionException e)
        {
            Logger?.LogCritical(LockRecursionText);
            Logger?.LogCritical("Exception and stacktrace: {Exception}, {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
        catch (Exception e)
        {
            Logger?.LogCritical("An exception has occurred: {Exception}, {StackTrace}",
                e.Message, e.StackTrace);
            throw;
        }
    }

    private enum LockType
    {
        Read,
        Write,
        UpgradeableRead,
        TryUpgradeableRead,
        TryRead,
        TryWrite,
    }
    #endregion
}


/// <summary>
/// Thread safety with an injected logger.  For documentation see <see cref="ThreadSafe">ThreadSafe</see>
/// </summary>
/// <typeparam name="TParent"></typeparam>
public abstract class ThreadSafe<TParent> : ThreadSafe where TParent : class
{
    // ReSharper disable once ContextualLoggerProblem
    protected ThreadSafe(ILogger<TParent> logger)
    {
        Logger = logger;
    }
}
