using System.Text.Json;

namespace DatafusLibrary.Core.Serialization;

public struct EntityJsonDocument
{
    public EntityDefinition[] def;
    public JsonElement[] data;
}

public struct EntityDefinitionField
{
    internal JsonElement name;
    internal JsonElement type;
    internal JsonElement? vectorTypes;
}

public struct EntityDefinition
{
    internal JsonElement memberName;
    internal JsonElement packageName;
    internal IEnumerable<EntityDefinitionField> fields;
}