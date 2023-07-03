using Microsoft.Extensions.Options;

namespace Csharp.Common.AppSettings;

/// <summary>
/// <para>
/// This wrapper class acts as a watcher for any App Settings classes that are tied to the IOptions{} interface.
/// </para>
///
/// <para>
/// When you register a set of classes by configuring a class to represent a structure in your App Settings, you use
/// IOptions as a dependency you inject into your class.  However, IOptions does not give you updated values if
/// the appsettings.json file changed. However: IOptionsMonitor does.  For this mechanism to work within a singleton
/// environment, you must wrap similarly as shown below.
/// </para>
///
/// <para>
/// Given an app settings class, you must create a watcher for your class by extending this abstract class and defining
/// the type you want to watch for changes.  When the app settings file changes, the update will change the settings
/// property of the class and then whenever you fetch the settings property, it will have the updated value.  It is important
/// to have a calculated get parameter or to always get the Settings from the watcher rather than assign in your
/// constructor.
/// </para>
/// </summary>
/// 
/// <example>
/// Full Example:
/// <code>
/// class MySettings
/// {
///    public int MySetting { get; set; } 
/// }
/// 
/// ... // configuring service
/// services.Configure{MySettings}(config.MySettings);
/// 
/// ... // creating watcher
/// public class MySettingsWatcher : AppSettingsWatcher{MySettings}
/// {
///   // this is all you need
///   public MySettingsWatcher(IOptionsMonitor{MySettings} settings) : base(settings) { }
/// }
///
/// ... // register the class
/// services.AddSingleton{MySettingsWatcher}();
///
/// ... // now you can inject into your classes
/// </code>
/// </example>
/// <typeparam name="T">concrete type associated to the app settings configuration file</typeparam>
public abstract class AppSettingsWatcher<T> : IAppSettingsWatcher<T> where T : class, new()
{
    public virtual event Action<T>? OnChange;

    protected T AppSettings;

    protected AppSettingsWatcher(IOptionsMonitor<T> settings)
    {
        AppSettings = settings.CurrentValue;
        settings.OnChange(Listener);
    }

    private void Listener(T settings)
    {
        AppSettings = settings;
        OnChange?.Invoke(settings);
    }

    public virtual T Settings => AppSettings;
}