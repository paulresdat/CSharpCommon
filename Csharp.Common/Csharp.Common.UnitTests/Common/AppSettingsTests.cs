using System.Text.RegularExpressions;
using AutoMapper;
using Csharp.Common.AppSettings;
using Csharp.Common.UnitTesting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Csharp.Common.UnitTests.Common;

public class TestAppSettings1
{
    public string Attribute1 { get; set; } = default!;
    public string Attribute2 { get; set; } = default!;
}

public class TestAppSettings1Dto : TestAppSettings1
{
}

public interface ITestAppSettingsWatcher : IAppSettingsWatcher<TestAppSettings1> {}
public interface ITestAppSettings1WatcherSafe : IAppSettingsWatcherSafe<TestAppSettings1, TestAppSettings1Dto> {}

public class TestAppSettingsWatcher : AppSettingsWatcher<TestAppSettings1>, ITestAppSettingsWatcher
{
    public TestAppSettingsWatcher(IOptionsMonitor<TestAppSettings1> settings) : base(settings)
    {
    }
}

public class TestAppSettings1WatcherSafe : AppSettingsWatcherSafe<TestAppSettings1, TestAppSettings1Dto>, ITestAppSettings1WatcherSafe
{
    public TestAppSettings1WatcherSafe(IMapper mapper, IOptionsMonitor<TestAppSettings1> settings) : base(mapper, settings)
    {
    }
}

public class AppSettingsTests : BaseUnitTest
{
    public AppSettingsTests()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        ServiceCollection.Configure<TestAppSettings1>(builder.GetSection("TestAppSettings1"));
        AddAutoMapper(cfg =>
        {
            cfg.CreateMap<TestAppSettings1, TestAppSettings1Dto>();
        });
    }

    [Fact(DisplayName = "001 App Settings Watcher can retrieve the app settings class")]
    public void T001()
    {
        ServiceCollection.AddSingleton<ITestAppSettingsWatcher, TestAppSettingsWatcher>();

        var sp = GetNewServiceProvider;
        var watcher = sp.GetRequiredService<ITestAppSettingsWatcher>();

        var obj = watcher.Settings;
        Assert.NotNull(obj);

        Assert.Equal("123", obj.Attribute1);
        Assert.Equal("345", obj.Attribute2);
        Assert.Equal(typeof(TestAppSettings1), obj.GetType());
    }

    [Fact(DisplayName = "002 App settings safe watcher will retrieve the app settings class as a dto")]
    public void T002()
    {
        ServiceCollection.AddSingleton<ITestAppSettings1WatcherSafe, TestAppSettings1WatcherSafe>();
        
        var sp = GetNewServiceProvider;
        var watcher = sp.GetRequiredService<ITestAppSettings1WatcherSafe>();

        var obj = watcher.Settings;
        Assert.NotNull(obj);

        Assert.Equal("123", obj.Attribute1);
        Assert.Equal("345", obj.Attribute2);
        Assert.Equal(typeof(TestAppSettings1Dto), obj.GetType());
    }

    [Fact(DisplayName =
        "003 When a change comes in, the appsettings will return the new value when asked for and invoke the event")]
    public void T003()
    {
        ServiceCollection.AddSingleton<ITestAppSettingsWatcher, TestAppSettingsWatcher>();

        var onChange = false;
        var sp = GetNewServiceProvider;
        var watcher = sp.GetRequiredService<ITestAppSettingsWatcher>();
        watcher.OnChange += settings1 =>
        {
            onChange = true;
        };

        var obj = watcher.Settings;
        Assert.Equal("123", obj.Attribute1);
        Assert.Equal("345", obj.Attribute2);

        var oldFile = new List<string>();
        using (var input = File.OpenText("appsettings.json"))
        {
            using (var output = new StreamWriter("output.tmp.txt"))
            {
                string? line;
                while (null != (line = input.ReadLine()))
                {
                    oldFile.Add(line);
                    if (line.Contains("123"))
                    {
                        line = Regex.Replace(line, "123", "789");
                    }
                    output.WriteLine(line);
                }
            }
        }


        if (File.Exists("appsettings.cache.json"))
        {
            File.Delete("appsettings.cache.json");
        }
        File.Move("appsettings.json", "appsettings.cache.json");
        File.Move("output.tmp.txt", "appsettings.json");

        Thread.Sleep(5000);
        obj = watcher.Settings;
        Assert.Equal("789", obj.Attribute1);
        Assert.Equal("345", obj.Attribute2);
        Assert.True(onChange);

        File.Delete("appsettings.json");
        File.Move("appsettings.cache.json", "appsettings.json");
        Thread.Sleep(5000);
    }
    
    
    [Fact(DisplayName =
        "004 When a change comes in for safe watcher, the appsettings will return the new value when asked for and invoke the event")]
    public void T004()
    {
        ServiceCollection.AddSingleton<ITestAppSettings1WatcherSafe, TestAppSettings1WatcherSafe>();

        TestAppSettings1Dto? onChange = null;
        var sp = GetNewServiceProvider;
        var watcher = sp.GetRequiredService<ITestAppSettings1WatcherSafe>();
        watcher.OnChangeSafe += settings1 =>
        {
            onChange = settings1;
        };

        var obj = watcher.Settings;
        Assert.Equal("123", obj.Attribute1);
        Assert.Equal("345", obj.Attribute2);

        var oldFile = new List<string>();
        using (var input = File.OpenText("appsettings.json"))
        {
            using (var output = new StreamWriter("output.tmp.txt"))
            {
                string? line;
                while (null != (line = input.ReadLine()))
                {
                    oldFile.Add(line);
                    if (line.Contains("123"))
                    {
                        line = Regex.Replace(line, "123", "789");
                    }
                    output.WriteLine(line);
                }
            }
        }


        if (File.Exists("appsettings.cache.json"))
        {
            File.Delete("appsettings.cache.json");
        }
        File.Move("appsettings.json", "appsettings.cache.json");
        File.Move("output.tmp.txt", "appsettings.json");

        Thread.Sleep(5000);
        obj = watcher.Settings;
        Assert.Equal("789", obj.Attribute1);
        Assert.Equal("345", obj.Attribute2);
        Assert.NotNull(onChange);

        File.Delete("appsettings.json");
        File.Move("appsettings.cache.json", "appsettings.json");
        Thread.Sleep(5000);
    }

    [Fact(DisplayName =
        "005 When subscribing to the unsafe event, an exception is thrown for the safe watcher")]
    public void T005()
    {
        ServiceCollection.AddSingleton<ITestAppSettings1WatcherSafe, TestAppSettings1WatcherSafe>();
        var sp = GetNewServiceProvider;
        var watcher = sp.GetRequiredService<ITestAppSettings1WatcherSafe>();

        Assert.Throws<InvalidOperationException>(() =>
        {
            watcher.OnChange += settings1 => { };
        });
    }
}