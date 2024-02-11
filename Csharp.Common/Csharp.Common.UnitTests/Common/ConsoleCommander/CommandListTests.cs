using System.Diagnostics.CodeAnalysis;
using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities.ConsoleCommander;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Csharp.Common.UnitTests.Common.ConsoleCommander;

[ExcludeFromCodeCoverage]
public class CommandListTests : BaseUnitTest
{
    public CommandListTests()
    {
        ServiceCollection.AddSingleton<IConsoleCommandList, CommandList>();
    }

    [Fact(DisplayName = "001 Running a simple command indeed runs it")]
    public async Task T001()
    {
        var sp = GetNewServiceProvider;
        var list = sp.GetRequiredService<IConsoleCommandList>();
        var test = false;
        list.AddCommand("tester", "Testing", () => { test = true; });
        await list.RunCommandAsync("tester");
        test.Should().Be(true);
    }

    [Fact(DisplayName = "001.1 Running an empty command does nothing")]
    public async Task T001_1()
    {
        var sp = GetNewServiceProvider;
        var list = sp.GetRequiredService<IConsoleCommandList>();
        await list.RunCommandAsync("");
    }

    [Fact(DisplayName = "002 Running a command with passed args and regex indeed runs")]
    public async Task T002()
    {
        var sp = GetNewServiceProvider;
        var list = sp.GetRequiredService<IConsoleCommandList>();
        var test = "";
        list.AddCommand("testing args", "Testing with args", regex: @"testing\s(\w+)", action: (s) =>
        {
            test = s;
        });
        await list.RunCommandAsync("testing data");
        test.Should().Be("testing data");
    }

    [Fact(DisplayName = "003 Running a simple async command with passed args and regex indeed runs")]
    public async Task T003()
    {
        var sp = GetNewServiceProvider;
        var list = sp.GetRequiredService<IConsoleCommandList>();
        var test = false;
        list.AddCommand("testing", "Testing with args", async () =>
        {
            await Task.Run(() =>
            {
                test = true;
            });
        });
        await list.RunCommandAsync("testing");
        test.Should().Be(true);
    }

    [Fact(DisplayName = "004 Running an async command with passed args and regex indeed runs")]
    public async Task T004()
    {
        var sp = GetNewServiceProvider;
        var list = sp.GetRequiredService<IConsoleCommandList>();
        var test = "";
        list.AddCommand("testing args", "Testing with args", regex: @"testing\s(\w+)", action: async (s) =>
        {
            await Task.Run(() => test = s);
        });
        await list.RunCommandAsync("testing data");
        test.Should().Be("testing data");
    }

    [Fact(DisplayName = "005 When an invalid command is found, a command list exception is thrown")]
    public void T005()
    {
        var sp = GetNewServiceProvider;
        var list = sp.GetRequiredService<IConsoleCommandList>();
        Assert.ThrowsAsync<CommandListException>(async () =>
        {
            await list.RunCommandAsync("blah blash");
        });
    }

    [Fact(DisplayName = "006 Command list provides a print out of the existing commands it has")]
    public void T006()
    {
        var sp = GetNewServiceProvider;
        var list = sp.GetRequiredService<IConsoleCommandList>();
        list.AddCommand("test1", "testing 1", () => { });
        list.AddCommand("test2", "testing 2", () => { });
        var l = list.Commands;
        l[0][0].Should().Be("test1");
        l[0][1].Should().Be("testing 1");
        l[1][0].Should().Be("test2");
        l[1][1].Should().Be("testing 2");
    }
}