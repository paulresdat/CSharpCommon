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
    // private readonly string _lockRecursionText;
    private const string LockRecursionText = 
        "A read lock recursion exception has been thrown while writing and is a critical error for the service to continue running, shutting down";

    /// <summary>
    /// Can be an extremely important element to check for.  Returns whether the object is on read a lock.
    /// </summary>
    protected bool InRead => _lockSlim.IsReadLockHeld;
    /// <summary>
    /// Can be an extremely important element to check for.  Returns whether the object is on read/write a lock.
    /// </summary>
    protected bool InWrite => _lockSlim.IsWriteLockHeld;
    protected bool InReadOrWrite => InRead || InWrite;

    protected ThreadSafe()
    {
    }

    /// <summary>
    /// Allows for a void read, in case you're manipulating data outside the scope of the read lock
    /// or some other kind of function that doesn't require a write lock on the object being locked
    /// </summary>
    /// <param name="action"></param>
    protected void EnterReadLock(Action action)
    {
        try
        {
            _lockSlim.EnterReadLock();
            try
            {
                action();
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }
        catch (LockRecursionException e)
        {
            Logger?.LogCritical(LockRecursionText);
            Logger?.LogCritical(e, "Exception and stacktrace");
            throw;
        }
        catch (Exception e)
        {
            Logger?.LogCritical("An exception has occurred: {Exception}, {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
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
        try
        {
            _lockSlim.EnterReadLock();
            try
            {
                return action();
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }
        catch (LockRecursionException e)
        {
            Logger?.LogCritical(LockRecursionText);
            Logger?.LogCritical(e, "Exception and stacktrace");
            throw;
        }
        catch (Exception e)
        {
            Logger?.LogCritical("An exception has occurred: {Exception}, {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
    }

    /// <summary>
    /// Enter a read write lock without an expected return value
    /// </summary>
    /// <param name="action"></param>
    protected void EnterReadWriteLock(Action action)
    {
        try
        {
            _lockSlim.EnterWriteLock();

            try
            {
                action();
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
        catch (LockRecursionException e)
        {
            Logger?.LogCritical(LockRecursionText);
            Logger?.LogCritical(e, "Exception and stacktrace");
            throw;
        }
        catch (Exception e)
        {
            Logger?.LogCritical("An exception has occurred: {Exception}, {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected T EnterReadWriteLock<T>(Func<T> action)
    {
        try
        {
            _lockSlim.EnterWriteLock();

            try
            {
                return action();
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }
        catch (LockRecursionException e)
        {
            Logger?.LogCritical(LockRecursionText);
            Logger?.LogCritical(e, "Exception and stacktrace");
            throw;
        }
        catch (Exception e)
        {
            Logger?.LogCritical("An exception has occurred: {Exception}, {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
    }
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
