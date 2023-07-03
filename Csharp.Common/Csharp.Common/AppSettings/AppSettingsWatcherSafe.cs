using AutoMapper;
using Microsoft.Extensions.Options;

namespace Csharp.Common.AppSettings;

public interface IAppSettingsWatcherSafe<T, TDto> : IAppSettingsWatcher<T>
    where T : class, new()
    where TDto : T, new()
{
    event Action<TDto> OnChangeSafe;
}


/// <summary>
/// <para>
/// This abstract class returns a safe (copied) data transfer object of the class that's associated to app settings.
/// It uses AutoMapper to map the options object to a new object so that changes won't reflect within the original
/// object.  This class behaves exactly the same way as <see cref="AppSettingsWatcher{T}">App Settings Watcher</see>
/// except it returns a copied object of the application settings object.
/// </para>
/// </summary>
/// <typeparam name="T">concrete class type</typeparam>
/// <typeparam name="TDto">the concrete class type to map to</typeparam>
public abstract class AppSettingsWatcherSafe<T, TDto> : AppSettingsWatcher<T>, IAppSettingsWatcherSafe<T, TDto>
    where T : class, new() 
    where TDto : T, new()
{
    public override event Action<T>? OnChange
    {
        add => throw new InvalidOperationException("Unsafe event subscribed to, use OnChangeSafe");
        remove => throw new InvalidOperationException("Unsafe event unsubscribed from, use OnChangeSafe");
    }

    public event Action<TDto>? OnChangeSafe;

    private readonly IMapper _mapper;
    protected AppSettingsWatcherSafe(
        IMapper mapper,
        IOptionsMonitor<T> settings) : base(settings)
    {
        _mapper = mapper;
        SubscribeEvent();
    }

    private void SubscribeEvent()
    {
        base.OnChange += s =>
        {
            OnChangeSafe?.Invoke(GetMap());
        };
    }

    private TDto GetMap()
    {
        return _mapper.Map<TDto>(AppSettings);
    }

    public override T Settings => GetMap();
}
