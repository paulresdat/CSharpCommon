using Csharp.Common.Interfaces;

namespace Csharp.Common.Utilities.ArgumentParsing;

public abstract record ArgumentParserDto : IRequest
{
  [ArgumentDefinition("h|help", "Help screen")]
  public bool Help { get; set; }
}
