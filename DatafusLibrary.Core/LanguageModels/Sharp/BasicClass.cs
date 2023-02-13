using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;

namespace DatafusLibrary.Core.LanguageModels.Sharp;

public class BasicClass
{
    public List<string> Usings { get; set; } = new();

    public string Namespace { get; set; } = string.Empty;

    public string? ClassModifier { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public string? ClassBase { get; set; }

    public List<PropertyDescriptor> Properties { get; set; } = new();
}