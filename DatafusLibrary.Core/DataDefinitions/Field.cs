namespace DatafusLibrary.Core.DataDefinitions;

public class Field
{
    public string? name { get; set; }
    public int type { get; set; }
    public List<Field>? complexType { get; set; }
}