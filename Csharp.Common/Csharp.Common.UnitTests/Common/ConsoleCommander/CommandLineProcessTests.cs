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
public class CommandLineProcessTests : BaseUnitTest
{
    public CommandLineProcessTests()
    {
        Mock<IConsoleOutput>();
        ServiceCollection.AddSingleton<ICommandLineProcessor, CommandLineProcessor>();
    }

    [Fact(DisplayName = "001 CLI Processor can read each key input until an enter is found")]
    public void T001()
    {
        Mock<IConsoleOutput>(m =>
        {
            var count = 0;
            m.Setup(x => x.ReadKey()).Returns(() =>
            {
                count++;
                if (count <= 3)
                {
                    return new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
                }
                return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
            });
        });

        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        clp.SetConsolePrompt("hi $>");
        var line = clp.ReadLine();
        line.Should().Be("aaa");
    }

    [Fact(DisplayName = "002 When a read key throws an exception, the exception is caught and reported")]
    public void T002()
    {
        var lines = new List<string>();
        Mock<IConsoleOutput>(m =>
        {
            m.Setup(x => x.WriteLine(It.IsAny<string>(), null, null)).Callback(
                (string? s, ConsoleColor? _, ConsoleColor? _) =>
                {
                    lines.Add(s ?? string.Empty);
                }).Returns(m.Object);

            m.Setup(x => x.ReadKey()).Returns(() =>
            {
                throw new Exception("Check exception handling");
            });
        });

        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        Assert.Throws<Exception>(() => clp.ReadLine());
        lines.Should().Contain("Exception was thrown: ");
    }

    [Fact(DisplayName = "003 After reading and trying to reset the cancellation token, an invalid exception will occur")]
    public void T003()
    {
        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        // we can set it initially
        clp.TokenSource = new CancellationTokenSource();
        // setting it again will cause it to error out
        Assert.Throws<InvalidOperationException>(() => 
            clp.TokenSource = new CancellationTokenSource());
    }

    [Fact(DisplayName = "004 You can set the cancellation token only when it has already been canceled")]
    public void T004()
    {
        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        // we can set it initially
        clp.TokenSource = new CancellationTokenSource();
        // setting it again will cause it to error out
        Assert.Throws<InvalidOperationException>(() => 
            clp.TokenSource = new CancellationTokenSource());
        clp.TokenSource.Cancel();
        Thread.Sleep(100);
        clp.TokenSource = new CancellationTokenSource();
    }

    [Fact(DisplayName = "005 When pushing arrow up, it will populate the last command in the list from its history")]
    public void T005()
    {
        var count = 0;
        Mock<IConsoleOutput>(m =>
        {
            m.Setup(x => x.ReadKey()).Returns(() =>
            {
                count++;
                if (count == 5)
                {
                    // that means we hit this again after another read line
                    return new ConsoleKeyInfo((char) 0x30, ConsoleKey.UpArrow, false, false, false);
                }

                if (count > 5)
                {
                    return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
                }
                if (count <= 3)
                {
                    return new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
                }
                return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
            });
        });
        
        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        var line = clp.ReadLine();
        line.Should().Be("aaa");
        var shouldBeSame = clp.ReadLine();
        line.Should().Be(shouldBeSame);
    }

    [Fact(DisplayName = "006 The user can enter in some info, arrow left to splice in characters, arrow right to add more characters before hitting enter")]
    public void T006()
    {
        Mock<IConsoleOutput>(m =>
        {
            var count = 0;
            m.Setup(x => x.ReadKey()).Returns(() =>
            {
                count++;
                if (count <= 3)
                {
                    return new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
                }

                if (count == 4 || count == 5)
                {
                    return new ConsoleKeyInfo((char) 0x28, ConsoleKey.LeftArrow, false, false, false);
                }

                if (count == 6 || count == 7)
                {
                    return new ConsoleKeyInfo('z', ConsoleKey.Z, false, false, false);
                }

                if (count == 8 || count == 9)
                {
                    return new ConsoleKeyInfo((char) 0x29, ConsoleKey.RightArrow, false, false, false);
                }

                if (count == 10)
                {
                    return new ConsoleKeyInfo('y', ConsoleKey.Y, false, false, false);
                }

                return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
            });
        });
        
        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        var line = clp.ReadLine();
        line.Should().Be("azzaay");
    }

    [Fact(DisplayName = "007 The user can browse the history with both up and down arrows and push return to select that history command")]
    public void T007()
    {
        var count = 0;
        var triggerDownArrow = false;
        var nowDownArrow = false;
        Mock<IConsoleOutput>(m =>
        {
            var hitDownArrow = false;
            m.Setup(x => x.ReadKey()).Returns(() =>
            {
                if (nowDownArrow && !hitDownArrow)
                {
                    hitDownArrow = true;
                    return new ConsoleKeyInfo((char) 0x31, ConsoleKey.DownArrow, false, false, false);
                }
                if (hitDownArrow)
                {
                    return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
                }

                count++;
                if (count <= 3)
                {
                    return new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
                }

                if (count == 4 || count == 8 || count == 12)
                {
                    return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
                }

                if (count <= 7)
                {
                    return new ConsoleKeyInfo('b', ConsoleKey.A, false, false, false);
                }

                if (count <= 12)
                {
                    return new ConsoleKeyInfo('c', ConsoleKey.A, false, false, false);
                }

                if (count == 13 || count == 14)
                {
                    return new ConsoleKeyInfo((char) 0x30, ConsoleKey.UpArrow, false, false, false);
                }

                if (count == 15 && triggerDownArrow)
                {
                    return new ConsoleKeyInfo((char) 0x31, ConsoleKey.DownArrow, false, false, false);
                }

                return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
            });
        });
        
        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        var line = clp.ReadLine();
        line.Should().Be("aaa");
        line = clp.ReadLine();
        line.Should().Be("bbb");
        line = clp.ReadLine();
        line.Should().Be("ccc");
        // we go up twice, should be "bbb"
        line = clp.ReadLine();
        line.Should().Be("bbb");
        // reset
        count = 0;
        clp.ReadLine();
        clp.ReadLine();
        clp.ReadLine();
        triggerDownArrow = true;
        // go up twice again and then down one, should be "ccc"
        line = clp.ReadLine();
        line.Should().Be("ccc");

        nowDownArrow = true;
        clp.ReadLine();
    }

    [Fact(DisplayName = "008 User can hit backspace to delete what they've done")]
    public void T008()
    {
        var count = 0;
        Mock<IConsoleOutput>(m =>
        {
            m.Setup(x => x.ReadKey()).Returns(() =>
            {
                count++;
                if (count <= 3)
                {
                    return new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false);
                }

                if (count == 4)
                {
                    return new ConsoleKeyInfo((char)0x08, ConsoleKey.Backspace, false, false, false);
                }

                if (count == 5)
                {
                    return new ConsoleKeyInfo('b', ConsoleKey.B, false, false, false);
                }
                return new ConsoleKeyInfo('\n', ConsoleKey.Enter, false, false, false);
            });
        });

        var sp = GetNewServiceProvider;
        var clp = sp.GetRequiredService<ICommandLineProcessor>();
        var line = clp.ReadLine();
        line.Should().Be("aab");
    }
}