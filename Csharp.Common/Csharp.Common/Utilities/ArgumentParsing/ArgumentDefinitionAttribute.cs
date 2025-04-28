namespace Csharp.Common.Utilities.ArgumentParsing;

public class ArgumentDefinitionAttribute : Attribute
{
    public string ArgumentName { get; set; }
    public string Description { get; set; }

    public ArgumentDefinitionAttribute(string name, string description)
    {
        // probably should validate but none for now
        ArgumentName = name;
        Description = description;
    }
}
