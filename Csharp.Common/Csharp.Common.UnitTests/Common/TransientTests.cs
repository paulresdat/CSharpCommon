using System.Diagnostics.CodeAnalysis;
using Csharp.Common.Services;
using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Csharp.Common.UnitTests.Common;

[ExcludeFromCodeCoverage]
public class TransientTests : BaseUnitTest
{
    public TransientTests(ITestOutputHelper output)
    {
        ServiceCollection.AddTransient<TransientObject>();
        ServiceCollection.AddSingleton<ITransientServiceProvider, TransientServiceProvider>();
    }

    [Fact(DisplayName = 
        "001 When a transient object has a the IDisposable interface, it is not called after the object has " +
        "been fetched from the scoped service provider")]
    public void T001()
    {
        // getting a disposable transient does not called dispose after returning out of using statement
        var sp = GetNewServiceProvider;
        var tsp = sp.GetRequiredService<ITransientServiceProvider>();
        var obj = tsp.GetRequiredTransient<TransientObject>();
        obj.Data.Should().BeEmpty();
    }

    [Fact(DisplayName = "002 When a transient isn't registered and required is not used, a null value is returned")]
    public void T002()
    {
        // getting a disposable transient does not called dispose after returning out of using statement
        var sp = GetNewServiceProvider;
        var tsp = sp.GetRequiredService<ITransientServiceProvider>();
        var obj = tsp.GetTransient<UnregisteredTransientObject>();
        obj.Should().BeNull();
    }
    
    private class TransientObject : ITransientService, IDisposable
    {
        public string Data { get; set; } = "";

        public void Dispose()
        {
            Data = "Disposed";
        }
    }

    private class UnregisteredTransientObject : ITransientService, IDisposable
    {
        public string Data { get; set; } = "";

        public void Dispose()
        {
            Data = "Disposed";
        }
    }
}
