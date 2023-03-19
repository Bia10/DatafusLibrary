using System.Text;
using System.Text.Json;
using DatafusLibrary.Core.IO;

namespace DatafusLibrary.Core.Serialization;

public static class JsonDocumentSerializer
{
    public static async Task<JsonDocument> GetJsonDocument(string jsonFilePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(jsonFilePath);

        var json = await FileReader.ReadAllAsync(jsonFilePath);

        ArgumentException.ThrowIfNullOrEmpty(json);

        var utf8Json = new MemoryStream(Encoding.UTF8.GetBytes(json));

        try
        {
            return await JsonDocument.ParseAsync(utf8Json);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
        finally
        {
            await utf8Json.DisposeAsync();
        }
    }

    public static IEnumerable<EntityDefinitionField> GetEntityDefinitionFields(this JsonElement jsonElement)
    {
        ArgumentNullException.ThrowIfNull(jsonElement);

        var result = new List<EntityDefinitionField>();

        if (!jsonElement.ValueKind.Equals(JsonValueKind.Array))
            throw new ArgumentException("Json element is not of value kind array!", nameof(jsonElement));

        foreach (var element in jsonElement.EnumerateArray())
        {
            var fieldDefinition = new EntityDefinitionField
            {
                name = element.GetProperty("name"),
                type = element.GetProperty("type"),
                vectorTypes = element.TryGetProperty("vectorTypes", out var vectorTypes) ? vectorTypes : null
            };

            result.Add(fieldDefinition);
        }

        return result;
    }

    public static IEnumerable<EntityDefinition> GetEntityDefinitions(IEnumerable<JsonElement> entityDefinitionElements)
    {
        ArgumentNullException.ThrowIfNull(entityDefinitionElements);

        var definitionElements = entityDefinitionElements as JsonElement[] ?? entityDefinitionElements.ToArray();

        var result = new List<EntityDefinition>();

        try
        {
            result.AddRange(
                definitionElements.Select(
                    jsonElement => new EntityDefinition
                    {
                        memberName = jsonElement.GetProperty("memberName"),
                        packageName = jsonElement.GetProperty("packageName"),
                        fields = jsonElement.GetProperty("fields").GetEntityDefinitionFields()
                    }));

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public static IEnumerable<JsonElement> GetDefinitionElements(JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(jsonDocument);

        var result = new List<JsonElement>();

        try
        {
            var defElement = jsonDocument.RootElement.GetProperty("def");

            result.AddRange(defElement.EnumerateArray());

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public static IEnumerable<JsonElement> GetDataElements(JsonDocument jsonDocument)
    {
        ArgumentNullException.ThrowIfNull(jsonDocument);

        var result = new List<JsonElement>();

        try
        {
            var dataElement = jsonDocument.RootElement.GetProperty("data");

            result.AddRange(dataElement.EnumerateArray());

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}