using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Csharp.Common.UnitTests.Common;

public class DebugOutputTests : BaseUnitTest
{
    public DebugOutputTests()
    {
        ServiceCollection.AddSingleton<HiDebug>();
        ServiceCollection.AddSingleton<HiDebug2>();
    }

    [Fact(DisplayName = "001 DebugOutput Console write pipes to the debug output delegate with default preample")]
    public void T001()
    {
        var sp = GetNewServiceProvider;
        var dbg = sp.GetRequiredService<HiDebug>();
        dbg.DebugOutputDelegate += message =>
        {
            message.Should().Be("HiDebug: Hi there");
        };
        dbg.ConsoleWrite("Hi there");
    }

    [Fact(DisplayName = "002 Preamble can be overwritten")]
    public void T002()
    {
        var sp = GetNewServiceProvider;
        var dbg = sp.GetRequiredService<HiDebug>();
        dbg.SetPreamble("Testing 123");
        dbg.DebugOutputDelegate += message =>
        {
            message.Should().Be("Testing 123: Hi there");
        };
        dbg.ConsoleWrite("Hi there");
    }

    [Fact(DisplayName = "003 The appropriate type supplied to DebugOutput<T> is the default preamble")]
    public void T003()
    {
        var sp = GetNewServiceProvider;
        var dbg = sp.GetRequiredService<HiDebug2>();
        dbg.DebugOutputDelegate += message =>
        {
            message.Should().Be("HiDebug2: Hi there");
        };
        dbg.ConsoleWrite("Hi there");
    }

    [Fact(DisplayName = "004 Preamble set to empty excludes the colon in the message")]
    public void T004()
    {
        var sp = GetNewServiceProvider;
        var dbg = sp.GetRequiredService<HiDebug2>();
        dbg.SetPreamble("");
        dbg.DebugOutputDelegate += message =>
        {
            message.Should().Be("Hi there");
        };
        dbg.ConsoleWrite("Hi there");
    }

    private class HiDebug : DebugOutput
    {
    }

    private class HiDebug2 : DebugOutput<HiDebug2>
    {
    }
}

