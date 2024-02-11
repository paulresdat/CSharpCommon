using System.Diagnostics.CodeAnalysis;
using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities;
using Csharp.Common.Utilities.ConsoleCommander;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Csharp.Common.UnitTests.Common.ConsoleCommander;

[ExcludeFromCodeCoverage]
public class ConsoleCommanderTests : BaseUnitTest
{
    public ConsoleCommanderTests()
    {
        Mock<IConsoleOutput>();
        Mock<ICommandLineProcessor>();
        ServiceCollection.AddSingleton<IConsoleCommandList, CommandList>();
        ServiceCollection.AddSingleton<CliCommanderTest>();
    }

    [Fact(DisplayName = "001 When the cli runs, initialize admin commands gets called and the admin splash screen gets called")]
    public void T001()
    {
        var addCommand = 0;
        Mock<IConsoleCommandList>(m =>
        {
            m.Setup(x => x.AddCommand(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Action>(),
                    It.IsAny<string?>()))
                .Returns(m.Object)
                .Callback(() =>
                {
                    addCommand++;
                });
            m.SetupGet(x => x.Commands).Returns(new List<string[]>
            {
                new[] { "1", "2" },
            });
        });

        var lines = new List<string>();
        Mock<IConsoleOutput>(m =>
        {
            m.Setup(x => x.WriteLine(It.IsAny<string>(), null, null)).Callback((string s, ConsoleColor? _, ConsoleColor? _) =>
            {
                lines.Add(s);
            });
        });

        Mock<ICommandLineProcessor>(m =>
            m.Setup(x => x.ReadLine()).Callback(() => 
                throw new QuitOutOfConsoleCommanderException()));

        var sp = GetNewServiceProvider;
        var cli = sp.GetRequiredService<CliCommanderTest>();
        cli.RunCli();
        lines.Count.Should().Be(2);
        lines.Should().Contain("Hello");
        addCommand.Should().Be(2);
    }

    [Fact(DisplayName = "002 Command list exception is caught and reported")]
    public void T002()
    {
        Mock<IConsoleCommandList>(m =>
        {
            m.Setup(x => x.AddCommand(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Action>(),
                    It.IsAny<string?>()))
                .Returns(m.Object);
            m.SetupGet(x => x.Commands).Returns(new List<string[]>
            {
                new[] { "1", "2" },
            });
        });

        var lines = new List<string>();
        Mock<IConsoleOutput>(m =>
        {
            m.Setup(x => x.WriteLine(It.IsAny<string>(), null, null)).Callback((string s, ConsoleColor? _, ConsoleColor? _) =>
            {
                lines.Add(s);
            });
        });

        var c = 0;
        Mock<ICommandLineProcessor>(m =>
            m.Setup(x => x.ReadLine()).Callback(() =>
            {
                if (c == 0)
                {
                    c++;
                    throw new CommandListException("test 123");
                }
                throw new QuitOutOfConsoleCommanderException();
            }));

        var sp = GetNewServiceProvider;
        var cli = sp.GetRequiredService<CliCommanderTest>();
        cli.RunCli();
        lines.Count.Should().BeGreaterThan(0);
        lines.Should().Contain("test 123");
    }

    [Fact(DisplayName = "003 Any exception is caught and reported")]
    public void T003()
    {
        Mock<IConsoleCommandList>(m =>
        {
            m.Setup(x => x.AddCommand(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Action>(),
                    It.IsAny<string?>()))
                .Returns(m.Object);
            m.SetupGet(x => x.Commands).Returns(new List<string[]>
            {
                new[] { "1", "2" },
            });
        });

        var lines = new List<string>();
        Mock<IConsoleOutput>(m =>
        {
            m.Setup(x => x.WriteLine(It.IsAny<string>(), null, null)).Callback((string s, ConsoleColor? _, ConsoleColor? _) =>
            {
                lines.Add(s);
            });
        });

        var c = 0;
        Mock<ICommandLineProcessor>(m =>
            m.Setup(x => x.ReadLine()).Callback(() =>
            {
                if (c == 0)
                {
                    c++;
                    throw new RandoException("testing rando exception");
                }
                throw new QuitOutOfConsoleCommanderException();
            }));

        var sp = GetNewServiceProvider;
        var cli = sp.GetRequiredService<CliCommanderTest>();
        cli.RunCli();
        lines.Count.Should().BeGreaterThan(0);
        lines.Should().Contain(x => x.Contains("testing rando exception"));
    }

    [Fact(DisplayName = "004 CLI Command line processor will execute each command when the processor returns that command name")]
    public void T004()
    {
        var lines = new List<string>();
        Mock<IConsoleOutput>(m =>
        {
            m.Setup(x => x.WriteLine(It.IsAny<string>(), null, null)).Callback((string s, ConsoleColor? _, ConsoleColor? _) =>
            {
                lines.Add(s);
            });
        });

        var c = 0;
        Mock<ICommandLineProcessor>(m =>
            m.Setup(x => x.ReadLine()).Returns(() =>
            {
                c++;
                if (c > 4)
                {
                    throw new QuitOutOfConsoleCommanderException();
                }
                return "test" + c;
            }));
        Thread.Sleep(1000);
        var sp = GetNewServiceProvider;
        var cli = sp.GetRequiredService<CliCommanderTest>();
        cli.RunCli();
        Thread.Sleep(1000);

        cli.Test1.Should().Be(1);
        cli.Test2.Should().Be(2);
        cli.Test3.Should().Be(3);
        cli.Test4.Should().Be(4);
    }

    private class RandoException : Exception
    {
        public RandoException(string message) : base(message)
        {
        }
    }

    private class CliCommanderTest : CliCommander
    {
        public int Test1 { get; set; }
        public int Test2 { get; set; }
        public int Test3 { get; set; }
        public int Test4 { get; set; }

        public CliCommanderTest(ICommandLineProcessor commandLineProcessor, IConsoleCommandList commandList, IConsoleOutput consoleOutput) : base(commandLineProcessor, commandList, consoleOutput)
        {
        }

        protected override void InitializeAdminCommands(ICommandListFluency commandList)
        {
            commandList.AddCommand("test1", "Test 1", () => { Test1 = 1; });
            commandList.AddCommand("test2", "Test 2", (s) => { Test2 = 2; });
            commandList.AddCommand("test3", "Test 3", async () => { await Task.Run(() => { Test3 = 3; }); });
            commandList.AddCommand("test4", "Test 4", async (s) => { await Task.Run(() => { Test4 = 4; }); });
        }

        protected override void AdminSplashScreen(IConsoleOutput consoleOutput)
        {
            consoleOutput.WriteLine("Hello");
        }
    }
}