using Csharp.Common.UnitTesting;
using Csharp.Common.Utilities.ArgumentParsing;
using FluentAssertions;
using Xunit;

namespace Csharp.Common.UnitTests.Common;

public class ArgumentParserTests : BaseUnitTest
{
  [Fact]
  public void Parses_typed_properties_and_aliases()
  {
    var parser = CreateTestParser();

    parser.ParseArguments(["-t", "--name", "Ada", "--count", "3"]);

    parser.Args.Test.Should().BeTrue();
    parser.Args.Name.Should().Be("Ada");
    parser.Args.Count.Should().Be(3);
  }

  [Fact]
  public void Uses_custom_interpreter_for_unsupported_property_types()
  {
    var parser = CreateTestParser();

    parser.ParseArguments(["--mode", "fast"]);

    parser.Args.Mode.Value.Should().Be("fast");
  }

  [Fact]
  public void Maps_enum_values_case_insensitively()
  {
    var parser = CreateTestParser();

    parser.ParseArguments(["--speed", "FAST"]);

    parser.Args.Speed.Should().Be(TestSpeed.Fast);
  }

  [Fact]
  public void Maps_nullable_enum_values()
  {
    var parser = CreateTestParser();

    parser.ParseArguments(["--optional-speed", "slow"]);

    parser.Args.OptionalSpeed.Should().Be(TestSpeed.Slow);
  }

  private static ArgumentParser<TestArguments> CreateTestParser()
  {
    var parser = new ArgumentParser<TestArguments>();
    parser.CustomInterpreter(x => x.Mode, value => new TestMode(value));
    return parser;
  }

  [Fact]
  public void Parses_all_supported_property_types()
  {
    var parser = new ArgumentParser<AllTypesArguments>();

    parser.ParseArguments(
      [
        "--string", "text",
        "--boolean",
        "--nullable-boolean", "true",
        "--byte", "8",
        "--nullable-byte", "9",
        "--sbyte", "-10",
        "--nullable-sbyte", "-11",
        "--short", "-12",
        "--nullable-short", "-13",
        "--ushort", "14",
        "--nullable-ushort", "15",
        "--int", "-16",
        "--nullable-int", "-17",
        "--uint", "18",
        "--nullable-uint", "19",
        "--long", "-20",
        "--nullable-long", "-21",
        "--ulong", "22",
        "--nullable-ulong", "23",
        "--float", "1.5",
        "--nullable-float", "2.5",
        "--double", "3.5",
        "--nullable-double", "4.5",
        "--decimal", "5.5",
        "--nullable-decimal", "6.5",
        "--enum", "Fast",
        "--nullable-enum", "Slow",
      ]
    );

    parser.Args.String.Should().Be("text");
    parser.Args.Boolean.Should().BeTrue();
    parser.Args.NullableBoolean.Should().BeTrue();
    parser.Args.Byte.Should().Be(8);
    parser.Args.NullableByte.Should().Be(9);
    parser.Args.Sbyte.Should().Be(-10);
    parser.Args.NullableSbyte.Should().Be(-11);
    parser.Args.Short.Should().Be(-12);
    parser.Args.NullableShort.Should().Be(-13);
    parser.Args.Ushort.Should().Be(14);
    parser.Args.NullableUshort.Should().Be(15);
    parser.Args.Int.Should().Be(-16);
    parser.Args.NullableInt.Should().Be(-17);
    parser.Args.Uint.Should().Be(18);
    parser.Args.NullableUint.Should().Be(19);
    parser.Args.Long.Should().Be(-20);
    parser.Args.NullableLong.Should().Be(-21);
    parser.Args.Ulong.Should().Be(22);
    parser.Args.NullableUlong.Should().Be(23);
    parser.Args.Float.Should().Be(1.5f);
    parser.Args.NullableFloat.Should().Be(2.5f);
    parser.Args.Double.Should().Be(3.5);
    parser.Args.NullableDouble.Should().Be(4.5);
    parser.Args.Decimal.Should().Be(5.5m);
    parser.Args.NullableDecimal.Should().Be(6.5m);
    parser.Args.Enum.Should().Be(TestSpeed.Fast);
    parser.Args.NullableEnum.Should().Be(TestSpeed.Slow);
  }

  [Fact]
  public void Invokes_custom_callbacks_with_and_without_argument_values()
  {
    var parser = new CustomCallbackArgumentParser();

    parser.ParseArguments(["--record", "hello", "--flag"]);

    parser.Args.Record.Should().Be(new CallbackRecord("hello"));
    parser.Args.Flag.Should().Be(new CallbackFlag(true));
  }

  private record TestArguments : ArgumentParserDto
  {
    [ArgumentDefinition("t|test", "Enable test mode")]
    public bool Test { get; set; }

    [ArgumentDefinition("name=", "The name")]
    public string? Name { get; set; }

    [ArgumentDefinition("count=", "The count")]
    public int Count { get; set; }

    [ArgumentDefinition("mode=", "The mode")]
    public TestMode Mode { get; set; } = null!;

    [ArgumentDefinition("speed=", "The speed")]
    public TestSpeed Speed { get; set; }

    [ArgumentDefinition("optional-speed=", "The optional speed")]
    public TestSpeed? OptionalSpeed { get; set; }
  }

  private enum TestSpeed
  {
    Slow,
    Fast,
  }

  private record AllTypesArguments : ArgumentParserDto
  {
    [ArgumentDefinition("string=", "String")]
    public string? String { get; set; }

    [ArgumentDefinition("boolean", "Boolean")]
    public bool Boolean { get; set; }

    [ArgumentDefinition("nullable-boolean=", "Nullable boolean")]
    public bool? NullableBoolean { get; set; }

    [ArgumentDefinition("byte=", "Byte")]
    public byte Byte { get; set; }

    [ArgumentDefinition("nullable-byte=", "Nullable byte")]
    public byte? NullableByte { get; set; }

    [ArgumentDefinition("sbyte=", "Sbyte")]
    public sbyte Sbyte { get; set; }

    [ArgumentDefinition("nullable-sbyte=", "Nullable sbyte")]
    public sbyte? NullableSbyte { get; set; }

    [ArgumentDefinition("short=", "Short")]
    public short Short { get; set; }

    [ArgumentDefinition("nullable-short=", "Nullable short")]
    public short? NullableShort { get; set; }

    [ArgumentDefinition("ushort=", "Ushort")]
    public ushort Ushort { get; set; }

    [ArgumentDefinition("nullable-ushort=", "Nullable ushort")]
    public ushort? NullableUshort { get; set; }

    [ArgumentDefinition("int=", "Int")]
    public int Int { get; set; }

    [ArgumentDefinition("nullable-int=", "Nullable int")]
    public int? NullableInt { get; set; }

    [ArgumentDefinition("uint=", "Uint")]
    public uint Uint { get; set; }

    [ArgumentDefinition("nullable-uint=", "Nullable uint")]
    public uint? NullableUint { get; set; }

    [ArgumentDefinition("long=", "Long")]
    public long Long { get; set; }

    [ArgumentDefinition("nullable-long=", "Nullable long")]
    public long? NullableLong { get; set; }

    [ArgumentDefinition("ulong=", "Ulong")]
    public ulong Ulong { get; set; }

    [ArgumentDefinition("nullable-ulong=", "Nullable ulong")]
    public ulong? NullableUlong { get; set; }

    [ArgumentDefinition("float=", "Float")]
    public float Float { get; set; }

    [ArgumentDefinition("nullable-float=", "Nullable float")]
    public float? NullableFloat { get; set; }

    [ArgumentDefinition("double=", "Double")]
    public double Double { get; set; }

    [ArgumentDefinition("nullable-double=", "Nullable double")]
    public double? NullableDouble { get; set; }

    [ArgumentDefinition("decimal=", "Decimal")]
    public decimal Decimal { get; set; }

    [ArgumentDefinition("nullable-decimal=", "Nullable decimal")]
    public decimal? NullableDecimal { get; set; }

    [ArgumentDefinition("enum=", "Enum")]
    public TestSpeed Enum { get; set; }

    [ArgumentDefinition("nullable-enum=", "Nullable enum")]
    public TestSpeed? NullableEnum { get; set; }
  }

  private record CustomCallbackArguments : ArgumentParserDto
  {
    [ArgumentDefinition("record=", "Record value")]
    public CallbackRecord Record { get; set; } = null!;

    [ArgumentDefinition("flag", "Record flag")]
    public CallbackFlag Flag { get; set; } = null!;
  }

  private class CustomCallbackArgumentParser
    : ArgumentParser<CustomCallbackArguments>
  {
    protected override void SetCustomCallbacks()
    {
      SetCustomCallback(
        args => args.Record,
        (args, value) => args.Record = new CallbackRecord(value)
      );
      SetCustomCallback(
        args => args.Flag,
        args => args.Flag = new CallbackFlag(true)
      );
    }
  }

  private sealed record CallbackRecord(string Value);

  private sealed record CallbackFlag(bool IsSet);

  private sealed class TestMode(string value)
  {
    public string Value { get; } = value;
  }
}
