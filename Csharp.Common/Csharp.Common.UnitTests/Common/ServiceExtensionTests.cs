using System.Diagnostics.CodeAnalysis;
using Csharp.Common.Extensions;
using Csharp.Common.UnitTesting;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Csharp.Common.UnitTests.Common;

[ExcludeFromCodeCoverage]
public class ServiceExtensionTests : BaseUnitTest
{
    public ServiceExtensionTests()
    {
    }

    [Fact(DisplayName = "001 Refreshing singletons works as expected")]
    public void T001()
    {
        Services.AddSingleton<MySingleton>();
        var sp = GetNewServiceProvider;
        var ms = sp.GetRequiredService<MySingleton>();
        ms.Id.Should().Be(-1);
        ms.Id = 1;
        Services.RefreshSingleton(ms);
        sp = GetNewServiceProvider;
        ms = sp.GetRequiredService<MySingleton>();
        ms.Id.Should().Be(1);
        
        Services.RefreshSingleton<MySingleton>();
        sp = GetNewServiceProvider;
        ms = sp.GetRequiredService<MySingleton>();
        ms.Id.Should().Be(-1);
    }

    [Fact(DisplayName = "002 Refreshing singletons with interface works as expected")]
    public void T002()
    {
        Services.AddSingleton<IMySingleton, MySingleton>();
        var sp = GetNewServiceProvider;
        var ms = sp.GetRequiredService<IMySingleton>();
        ms.Id.Should().Be(-1);
        ms.Id = 1;
        Services.RefreshSingleton(ms);
        sp = GetNewServiceProvider;
        ms = sp.GetRequiredService<IMySingleton>();
        ms.Id.Should().Be(1);

        Services.RefreshSingleton<IMySingleton, MySingleton>();
        sp = GetNewServiceProvider;
        ms = sp.GetRequiredService<IMySingleton>();
        ms.Id.Should().Be(-1);
    }

    private interface IMySingleton
    {
        int Id { get; set; }
    }

    private class MySingleton : IMySingleton
    {
        public int Id { get; set; } = -1;
    }
}