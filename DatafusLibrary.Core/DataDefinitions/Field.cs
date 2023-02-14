namespace DatafusLibrary.Core.DataDefinitions;

public class Field
{
    public string? name { get; set; }
    public int type { get; set; }
    public Field? vectorTypes { get; set; }
}