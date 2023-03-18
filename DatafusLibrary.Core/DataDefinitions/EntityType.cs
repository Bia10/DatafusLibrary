namespace DatafusLibrary.Core.DataDefinitions;

public class EntityType
{
    public string? MemberName { get; set; }
    public string? PackageName { get; set; }
    public IEnumerable<Field>? Fields { get; set; }
}