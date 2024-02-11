using System.Diagnostics.CodeAnalysis;
using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities.WaitFor;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Csharp.Common.UnitTests.Common;

[ExcludeFromCodeCoverage]
public class WaitForTests : BaseUnitTest
{
    public WaitForTests()
    {
        ServiceCollection.AddTransient<IWait, Wait<WaitResult>>();
    }

    [Fact(DisplayName = "001 When waiting, a result successful return is immediate and the result will return successful true")]
    public void T001()
    {
        var sp = GetNewServiceProvider;
        var wait = sp.GetRequiredService<IWait>();
        // when a wait returns success, the result object reflects this
        var result = wait.For(1, () => WaitStatus.Success);
        result.Success.Should().Be(true);
    }

    [Fact(DisplayName = "002 When waiting, a result error return is immediate and the result will return successful false")]
    public void T002()
    {
        var sp = GetNewServiceProvider;
        var wait = sp.GetRequiredService<IWait>();
        // when a wait returns success, the result object reflects this
        var result = wait.For(1, () => WaitStatus.Error);
        result.Success.Should().Be(false);
    }

    [Fact(DisplayName = "003 When waiting, A continue will iterate over the time and will timeout with a result of false")]
    public void T003()
    {
        var sp = GetNewServiceProvider;
        var wait = sp.GetRequiredService<IWait>();
        // when a wait returns success, the result object reflects this
        var result = wait.For(2, () => WaitStatus.Continue);
        result.Success.Should().Be(false);
    }

    [Fact(DisplayName = "004 When waiting, A continue will iterate over the allotted time specified and will timeout with a result of false")]
    public void T004()
    {
        var sp = GetNewServiceProvider;
        var wait = sp.GetRequiredService<IWait>();
        // when a wait returns success, the result object reflects this
        var count = 0;
        var result = wait.For(1, () =>
        {
            count++;
            return WaitStatus.Continue;
        }, 200);
        result.Success.Should().Be(false);
        count.Should().BeGreaterOrEqualTo(4);
    }

    [ExcludeFromCodeCoverage]
    private class WaitResult : IWaitResult
    {
        public bool Success { get; set; }
        public string? Reason { get; set; }
        public WaitStatus WaitStatus { get; set; }
    }
}