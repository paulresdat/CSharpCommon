using System.Reflection;
using AutoMapper;
using Csharp.Common.Extensions;
using MELT;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit.Abstractions;

namespace Csharp.Common.UnitTesting;

/// <summary>
/// <para>
/// This abstract class is an extremely helpful helper suite for building out your unit/integration testing classes with XUnit.
/// It shorthands several verbose and boilerplate heavy code that you have to write for every unit/integration test
/// class.  As an abstract, you can further add onto it by building your own custom integration/unit testing helper
/// functions that revolve around the basic premise of these methods.
/// </para>
///
/// <para>
/// With this suite you get these helpers that make your life better:
///
/// <list type="number">
/// <item><description>Awesome mock shorthands (seriously this alone is awesome)</description></item>
/// <item><description>Auto-automapper configurations :)</description></item>
/// <item><description>Service collection shorthands</description></item>
/// <item><description>MeltLogger shorthands</description></item>
/// </list>
///
/// These simple but powerful helper functions greatly reduce the amount of code you write and simplifies unit testing
/// logic scenarios making it easier to focus on the test more so than setting up the test which is significant, especially
/// for mocking.
/// </para>
///
/// <para>
/// Note that you should be familiar with unit testing to utilize this at its best.  It is recommended you have some
/// unit testing experience or have access to an experienced developer who has extensively written tests to get an
/// idea on how to best approach unit testing your code while utilizing this abstract class.
/// </para>
///
/// <para>
/// Below are several examples that
/// illustrate how this class can ease the pain of setting up unit/integration tests within xUnit specifically.  If
/// you are using a different unit testing framework, this can still prove infinitely useful by adapting the methods
/// to another base unit testing class that works with that unit testing framework.  At the time of this writing,
/// there's little wiki documentation on the bootstrapping and using this class appropriately, but we do have it
/// in our scope to eventually tackle a how-to.
/// </para>
///
/// <example>
/// Basic service collection example
/// <code>
/// 
/// public MyUnitTestingSuite : BaseUnitTest
/// {
///    public MyUnitTestingSuite(ITestOutputHelper output)
///    {
///       ServiceCollection.AddSingleton{IMyObject, MyObject}();
///    }
///
///    [Fact(DisplayName = "Your test")]
///    public void YourTest()
///    {
///       var sp = GetNewServiceProvider;
///       var myObject = sp.GetRequiredService{IMyObject}();
///       Assert.True(myObject.PerformTest());
///    }
/// }
/// </code>
/// </example>
///
/// 
/// <example>
/// Basic mock example.  You can test your mock is returned by fetching the option in the service provider.
/// The Mock{} shorthand automatically maps your object into your service collection, no more having to write
/// that boilerplate code every time, cleaning up your tests and making them much much more readable.
/// <code>
/// [Fact(DisplayName = "Your Test")]
/// public void YourTest()
/// {
///    Mock{IOptions{MySettings}}(m => {
///      m.SetupGet(x => x.Value)
///        .Returns(new MySettings {
///          DefaultValue = 1,
///        });
///    });
///
/// 
///    var sp = GetNewServiceProvider;
///    var settings = sp.GetRequiredService{IOptions{MySettings}}();
///    Assert.Equal(1, settings.Value.DefaultValue);
/// }
/// </code>
/// </example>
/// </summary>
public abstract partial class BaseUnitTest
{
    private readonly ITestLoggerFactory _loggerFactory;
    protected IEnumerable<LogEntry> LogEntries => _loggerFactory.Sink.LogEntries;
    // ServiceCollection CAN NOT be set outside of this abstract class
    // DO NOT REMOVE "private set;" and replace with "set;", this is by design
    protected IServiceCollection ServiceCollection { get; private set; }
    protected virtual IServiceProvider GetNewServiceProvider => ServiceCollection.BuildServiceProvider();
    
    protected BaseUnitTest()
    {
        _loggerFactory = TestLoggerFactory.Create();
        ServiceCollection = new ServiceCollection();
    }

    /// <summary>
    /// <para>Adding Automapper configuration.  Extremely helpful for setting up profiles with minimal code.</para>
    /// <para>You only need to call this once in your constructor.</para>
    /// </summary>
    /// 
    /// <example>
    /// <code>
    /// public Constructor()
    /// {
    ///    AddAutoMapper(cfg => {
    ///      cfg.AddProfile{ProfileOne}();
    ///      cfg.AddProfile{ProfileTwo}();
    ///      ... etc ...
    ///    });
    /// }
    /// </code>
    /// </example>
    /// <param name="profileAction"></param>
    protected void AddAutoMapper(Action<IMapperConfigurationExpression> profileAction)
    {
        var mapper = new MapperConfiguration(profileAction);
        ServiceCollection.AddSingleton(mapper);
        ServiceCollection.AddSingleton(mapper.CreateMapper());
    }

    /// <summary>
    /// <para>This is an extremely helpful function for registering melt loggers to inspect logging in your tests.</para>
    /// <para>You can look through your logs using the protected property <see cref="LogEntries">LogEntries</see>.
    /// Note as well that you can inspect the LogEntries and what logs were created in debug mode within your test.</para>
    /// <para>By creating specific logs, you can concentrate only on the logs of the class you wish to test, making logs
    /// far more easy to read in inspection, and more performant when running tests on them (less data to sift through
    /// if your app is log heavy)</para>
    /// </summary>
    ///
    /// <example>
    /// <code>
    /// public Constructor()
    /// {
    ///    // this essentially wraps the `ILogger{MyClass}` injection
    ///    // in the service collection.  Whenever `ILogger{MyClass}`
    ///    // is asked for, it will return the melt logger's wrapper.
    ///    AddMeltLogger{MyClass}();
    /// }
    /// </code>
    /// </example>
    /// <typeparam name="T">The concrete class of the logger, leave out ILogger</typeparam>
    protected void AddMeltLogger<T>() where T : class
    {
        var logger = _loggerFactory.CreateLogger<T>();
        ServiceCollection.RefreshSingleton(logger);
    }

    /// <summary>
    /// Mocks a singleton of the object using a function that returns the object itself.
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="T"></typeparam>
    [Obsolete("We don't need a returned object anymore")]
    protected void Mock<T>(Func<Mock<T>, Mock<T>> func) where T : class
    {
        var inst = (Mock<T>?) Activator.CreateInstance(typeof(Mock<T>));
        ServiceCollection.RefreshSingleton(func(inst!).Object);
    }

    /// <summary>
    /// Mocks a transient by using a function that expects a returned object.
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="T"></typeparam>
    [Obsolete("We don't need a returned object anymore")]
    protected void MockTransient<T>(Func<Mock<T>, Mock<T>> func) where T : class
    {
        var inst = (Mock<T>?) Activator.CreateInstance(typeof(Mock<T>));
        ServiceCollection.RefreshTransient(func(inst!).Object);
    }

    /// <summary>
    /// <para>The most used aspect of the mock methods: Mocking and providing the mock functionality in one scoped call.</para>
    /// <para>
    /// The beauty of this wrapper allows us to take away the boilerplate code that registers the mocked object
    /// into the service collection.  It reduces the clutter in your code and all you see written is the actual mock
    /// which means your code is more readable as a consequence.  Unit tests are clean with this approach.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// public Constructor()
    /// {
    ///   Mock{MyObject}(mockedObject => {
    ///     mockedObject.Setup(x => x.Action(It.IsAny{Interface}))
    ///       .Returns(true);
    ///   });
    /// }
    /// </code>
    /// </example>
    /// <param name="func">Action on the mock that needs to performed</param>
    /// <typeparam name="T">The concrete to mock</typeparam>
    protected void Mock<T>(Action<Mock<T>> func) where T : class
    {
        var inst = (Mock<T>?) Activator.CreateInstance(typeof(Mock<T>));
        func(inst!);
        ServiceCollection.RefreshSingleton(inst!.Object);
    }

    /// <summary>
    /// This follows a shorthand way to provide a mock without necessary boiler plate code around
    /// instantiating a new mock object.  It falls in line with design since it's a protected method
    /// in the base layout, increasing uniform readability in the unit tests.
    /// </summary>
    /// <param name="func"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    protected Mock<T> MockOf<T>(Action<Mock<T>>? func = null) where T : class
    {
        var inst = (Mock<T>?) Activator.CreateInstance(typeof(Mock<T>));
        if (func is not null)
        {
            func(inst!);
        }
        return inst!;
    }

    /// <summary>
    /// This is the shorthand version of either mocking an empty object and registering it, or
    /// providing an already mocked object to register to the service collection.  The latter is
    /// less used, except for very specific use case scenarios that come up only within context
    /// of the test.  Otherwise, you'll use this just to mock something that you don't want
    /// to have functionality attributed to, like an interface to keep a test from being an
    /// integration test.
    /// </summary>
    /// <param name="alreadyMocked">A possible mocked object, can be null or empty (most use cases)</param>
    /// <typeparam name="T">The object type to mock</typeparam>
    /// <exception cref="InvalidOperationException">If the object cannot be found within the assembly this exception will be thrown</exception>
    protected void Mock<T>(Mock<T>? alreadyMocked = null) where T : class
    {
        if (alreadyMocked != null!)
        {
            ServiceCollection.RefreshSingleton(alreadyMocked.Object);
        }
        else
        {
            var inst = (Mock<T>?)Activator.CreateInstance(typeof(Mock<T>));
            ServiceCollection.RefreshSingleton(inst?.Object ?? throw new InvalidOperationException());
        }
    }

    protected void MockOption<T>(T returnValue) where T : class
    {
        var inst = (Mock<IOptions<T>>?) Activator.CreateInstance(typeof(Mock<IOptions<T>>));
        inst!.Setup(x => x.Value).Returns(returnValue);
        ServiceCollection.RefreshSingleton(inst.Object);
    }

    protected void MockOption<T>(Action<Mock<IOptions<T>>> func) where T : class
    {
        var inst = (Mock<IOptions<T>>?) Activator.CreateInstance(typeof(Mock<IOptions<T>>));
        func(inst!);
        ServiceCollection.RefreshSingleton(inst!.Object);
    }

    /// <summary>
    /// For granularity, you can use this wrapper to wrap the exception that is thrown for a race condition
    /// on the Castle.Proxies.  However, once you disable parallelization using the DisableParallelization collection
    /// attribute, you no longer need this wrapper.
    /// </summary>
    /// <param name="func"></param>
    /// <param name="output"></param>
    protected void CastleProxyRaceConditionWrapper(Action func, ITestOutputHelper output = null!)
    {
        try
        {
            func();
        }
        catch (ReflectionTypeLoadException exc)
        {
            output.WriteLine(
                "Warning: a reflection type load exception has been caught.  Consider turning off parallelization " +
                "for your class if this error intermittently persists the rest of your tests");
            output.WriteLine(exc.Message);
            output.WriteLine(exc.StackTrace);
        }
    }
}
