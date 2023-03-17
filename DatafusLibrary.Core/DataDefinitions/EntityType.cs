namespace DatafusLibrary.Core.DataDefinitions;

public class EntityType
{
    public string? memberName { get; set; }
    public string? packageName { get; set; }
    public IEnumerable<Field>? fields { get; set; }
}