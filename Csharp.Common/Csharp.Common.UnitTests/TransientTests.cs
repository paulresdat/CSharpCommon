using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Csharp.Common.UnitTests;


public class TransientObject : ITransientService, IDisposable
{
    public string Data { get; set; } = "";

    public void Dispose()
    {
        Data = "Disposed";
    }
}

public class TransientTests : BaseUnitTest
{
    public TransientTests(ITestOutputHelper output)
    {
        ServiceCollection.AddTransient<TransientObject>();
        ServiceCollection.AddSingleton<ITransientServiceProvider, TransientServiceProvider>();
    }

    [Fact(DisplayName = "001 When a transient object has a the IDisposable interface, it is not called after the object has " +
                        "been fetched from the scoped service provider")]
    public void T001()
    {
        // getting a disposable transient does not called dispose after returning out of using statement
        var sp = GetNewServiceProvider;
        var tsp = sp.GetRequiredService<ITransientServiceProvider>();
        var obj = tsp.GetRequiredTransient<TransientObject>();
        Assert.Equal("", obj.Data);
    }
}
