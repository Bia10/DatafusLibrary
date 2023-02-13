namespace DatafusLibrary.Core.DataDefinitions;

public class EntityType
{
    public string? memberName { get; set; }
    public string? packageName { get; set; }
    public List<Field>? fields { get; set; }
}