namespace DatafusLibrary.SourceGenerators.Models.Sharp.Descriptors;

public class PropertyDescriptor
{
    public string AccessibilityLevel { get; set; } = "public";

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Initializer { get; set; } = null;

    public string? Accessors { get; set; } = "{ get; set; }";
}