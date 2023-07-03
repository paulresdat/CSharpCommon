using System;

namespace Csharp.Common.Utilities;

/// <summary>
/// The interface to extend if a direct inheritance isn't possible<br/><br/>
///
/// Sometimes you'll have classes that already inherit from a base class,
/// but you'll still want DebugOutput functionality available
/// </summary>
public interface IDebugOutput
{
    /// <summary>
    /// The abstract class requires some casting, here you have access to the delegate through an interface cast
    /// unless you extend the interface of your base class with this
    /// </summary>
    event DebugOutputDelegate? DebugOutputDelegate;
    /// <summary>
    /// You can copy the implementation details from the abstract class when implementing this method
    /// </summary>
    /// <param name="data"></param>
    void ConsoleWrite(params object[] data);
}

public delegate void DebugOutputDelegate(string message);

/// <summary>
/// <para>
/// Attach to any class so you can subscribe to the delegate and output data internally in your unit tests
/// </para>
///
/// <example>
/// Ie:
/// <code>
/// public class YourClass : DebugOutput { .. }
///
/// var yourClass = new YourClass();
/// var output = (ITestOutputHelper)_output;
/// yourClass.DebugOutputDelegate += (str) => {
///   output.WriteLine(str);
/// }
/// </code>
/// </example>
///
/// <example>
/// or by way of DI:
/// <code>
/// ServiceCollection.AddSingleton|IYourClass, YourClass|();
/// var yourClass = ServiceProvider.GetRequiredService|IYourClass|();
/// ((YourClass)yourClass).DebugOutputDelegate += (str) { .. }
/// </code>
/// </example>
/// </summary>
public abstract class DebugOutput
{
    public event DebugOutputDelegate? DebugOutputDelegate;
    protected string Preamble { get; set; } = "";
    private string Pre => _pT?.Name ?? "(n/a): ";
    private readonly Type? _pT;

    public DebugOutput()
    {
        _pT = GetType().BaseType;
    }

    protected void ConsoleWrite(params object[] data)
    {
        foreach (var obj in data)
        {
            DebugOutputDelegate?.Invoke(Pre + obj);
        }
    }
}

/// <summary>
/// If you are debugging multiple classes, you can pass the type with this and populate
/// a variable that defines the type for each console output for you.  Helps organize your
/// test output.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class DebugOutput<T> : DebugOutput
{
    protected DebugOutput()
    {
        var t = typeof(T).Name;
        Preamble = t + ": ";
    }
}
