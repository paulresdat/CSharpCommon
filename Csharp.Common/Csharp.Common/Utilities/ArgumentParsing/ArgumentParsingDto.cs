namespace Csharp.Common.Utilities.ArgumentParsing;

public abstract class ArgumentParsingDto
{
    [ArgumentDefinition("h|help", "Help screen")]
    public bool Help { get; set; }
}
