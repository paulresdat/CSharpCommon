using System.Diagnostics.CodeAnalysis;
using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities.ArgumentParsing;
using FluentAssertions;
using Xunit;

namespace Csharp.Common.UnitTests.Common;

[ExcludeFromCodeCoverage]
public class ArgumentParsingTests : BaseUnitTest
{
  public ArgumentParsingTests() { }

  [Theory(DisplayName = "001 Boolean options support each declared alias")]
  [InlineData("-t")]
  [InlineData("--test")]
  public void T001(string argument)
  {
    var t = new TestArgsWithProps();

    t.ParseArguments([argument]);

    t.Args.Test.Should().BeTrue();
  }

  [Theory(DisplayName = "002 Help supports both declared aliases")]
  [InlineData("-h")]
  [InlineData("--help")]
  public void T002(string argument)
  {
    var t = new TestArgsWithProps();

    t.ParseArguments([argument]);

    t.Args.Help.Should().BeTrue();
    t.AskedForHelp.Should().BeTrue();
  }

  [Fact(DisplayName = "003 Unknown arguments are reported without throwing")]
  public void T003()
  {
    var t = new TestArgsWithProps();

    var action = () => t.ParseArguments(["--unknown"]);

    action.Should().NotThrow();
    t.Args.Test.Should().BeFalse();
  }

  [Fact(DisplayName = "004 Missing values are reported without throwing")]
  public void T004()
  {
    var t = new TestArgsWithProps();

    var action = () => t.ParseArguments(["--test-int"]);

    action.Should().NotThrow();
    t.Args.TestInt.Should().Be(0);
  }

  [Theory(
    DisplayName = "005 All argument types are possible including custom params and custom callbacks"
  )]
  [InlineData("-t", null, "Test", typeof(bool), false)]
  [InlineData("--test", null, "Test", typeof(bool), false)]
  [InlineData("--test-str", "123", "TestStr", typeof(string), false)]
  [InlineData("--test-byte", "1", "TestByte", typeof(byte), false)]
  [InlineData("--test-short", "12", "TestShort", typeof(short), false)]
  [InlineData("--test-int", "123", "TestInt", typeof(int), false)]
  [InlineData("--test-long", "1234", "TestLong", typeof(long), false)]
  [InlineData("--test-type", "whatever", "TestNewType", typeof(Nullable), true)]
  [InlineData("--test-another-type", "whatever", "TestAnotherNewType", typeof(Nullable), true)]
  // [InlineData("--test-without-any-description", "1234", "TestWithoutAnyDescription", typeof(int), false)]
  public void T005(string arg, string? value, string propName, Type type, bool customCallback)
  {
    var t = new TestArgsWithProps();
    var args = new string[value == null ? 1 : 2];
    args[0] = arg;
    if (value is not null)
    {
      args[1] = value;
    }
    t.ParseArguments(args);

    var prop = t.Args.GetType().GetProperties().First(x => x.Name == propName);

    if (customCallback)
    {
      if (propName == "TestAnotherNewType")
      {
        // do nothing for now
        t.Args.TestAnotherNewType.PropOne.Should().Be(1);
        t.Args.TestAnotherNewType.PropTwo.Should().Be(8);
        return;
      }
      // do nothing for now
      t.Args.TestNewType.PropOne.Should().Be(1);
      t.Args.TestNewType.PropTwo.Should().Be(2);
      return;
    }

    var val = prop.GetValue(t.Args, null);
    if (type == typeof(bool))
    {
      val.Should().Be(true);
      return;
    }

    if (type == typeof(string))
    {
      val.Should().Be(value);
      return;
    }

    object? checkAgainst = null;
    if (type == typeof(byte))
    {
      byte.TryParse(value, out var i);
      checkAgainst = i;
    }

    if (type == typeof(short))
    {
      short.TryParse(value, out var i);
      checkAgainst = i;
    }

    if (type == typeof(int))
    {
      int.TryParse(value, out var i);
      checkAgainst = i;
    }

    if (type == typeof(long))
    {
      long.TryParse(value, out var i);
      checkAgainst = i;
    }

    val.Should().Be(checkAgainst);
  }

  [ExcludeFromCodeCoverage]
  private record TestArgProps : ArgumentParserDto
  {
    [ArgumentDefinition("t|test", "Testing bool")]
    public bool Test { get; set; }

    [ArgumentDefinition("test-str=", "Testing str")]
    public string? TestStr { get; set; }

    // testing numerics
    [ArgumentDefinition("test-byte=", "Testing byte")]
    public byte TestByte { get; set; }

    [ArgumentDefinition("test-short=", "Testing short")]
    public short TestShort { get; set; }

    [ArgumentDefinition("test-int=", "Testing int")]
    public int TestInt { get; set; }

    [ArgumentDefinition("test-long=", "Testing long")]
    public long TestLong { get; set; }

    [ArgumentDefinition("test-type=", "Testing type and custom callback")]
    public NewType TestNewType { get; set; } = default!;

    [ArgumentDefinition("test-another-type=", "Testing type and custom callback")]
    public NewType TestAnotherNewType { get; set; } = default!;

    // public int TestWithoutAnyDescription { get; set; }
  }

  [ExcludeFromCodeCoverage]
  private record NewType
  {
    public int PropOne { get; set; }
    public int PropTwo { get; set; }
  }

  /// <summary>
  /// NEW WAY :-)
  /// </summary>
  [ExcludeFromCodeCoverage]
  private class TestArgsWithProps : ArgumentParser<TestArgProps>
  {
    protected override void SetCustomCallbacks()
    {
      SetCustomCallback(
        x => x.TestNewType,
        (args) =>
        {
          var n = new NewType { PropOne = 1, PropTwo = 2 };
          args.TestNewType = n;
        }
      );
      SetCustomCallback(
        x => x.TestAnotherNewType,
        (args, s) =>
        {
          var n = new NewType { PropOne = 1, PropTwo = s.Length };
          args.TestAnotherNewType = n;
        }
      );
    }
  }
}
