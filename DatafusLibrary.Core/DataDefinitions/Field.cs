namespace DatafusLibrary.Core.DataDefinitions;

public class Field
{
    public string? Name { get; set; }
    public int Type { get; set; }
    public Field? VectorTypes { get; set; }
}