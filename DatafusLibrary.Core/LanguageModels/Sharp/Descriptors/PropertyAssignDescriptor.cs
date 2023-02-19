namespace DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;

public class PropertyAssignDescriptor
{
    public string Destination { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public ParameterDescriptor ToCamelCase()
    {
        return new ParameterDescriptor
        {
            Name = char.ToLower(Source[0]) + Source[1..],
            Type = Type
        };
    }
}