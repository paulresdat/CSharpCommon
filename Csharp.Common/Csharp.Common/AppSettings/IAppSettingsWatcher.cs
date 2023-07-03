namespace Csharp.Common.AppSettings;

/// <summary>
/// This is used for abstract classes that require any kind of settings to be defined if there is an interface that
/// describes the expected settings.  It shouldn't be used unless you know that you require the interface instead of the
/// concrete.  99% of the time, you will not need this.  For an example of use see the TCSIM Message Bus Repository base
/// class.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAppSettingsWatcherBase<out T> where T : class
{
    T Settings { get; }
}

/// <summary>
/// <para>
/// This interface can help define another interface of a class object that extends the abstract app settings watcher.
/// Why would you want to do that?  Because of unit testing.  When unit testing, you can't directly mock an object that
/// extends the abstract watcher class because it will complain about the lack of a parameterless constructor.  Instead you
/// want to use an interface.  Use this interface to extend your class's interface definition.
/// </para>
///
/// <para>Example code below:</para>
/// </summary>
///
/// <example>
/// <code>
/// public interface IMyAppSettingsWatcher : IAppSettingsWatcher{MySettings} { }
/// public class MyAppSettingsWatcher : AppSettingsWatcher{MySettings}, IMyAppSettingsWatcher { ... }
/// 
/// ... // now you can register it with an interface
/// services.AddSingleton{IMyAppSettingsWatcher, MyAppSettingsWatcher}();
/// 
/// ... // now you can mock it in your unit tests
/// Mock{IMyAppSettingsWatcher}(m => {
///   m.SetupGet(x => x.Settings)
///     .Returns(new MySettings() { ... });
/// });
/// </code>
/// </example>
/// <typeparam name="T"></typeparam>
public interface IAppSettingsWatcher<out T> : IAppSettingsWatcherBase<T> where T: class, new()
{
    event Action<T>? OnChange;
}