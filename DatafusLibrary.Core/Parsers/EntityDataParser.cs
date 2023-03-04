using System.Text.Json;
using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.Core.Parsers;

public static class EntityDataParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static async Task<T?> GetDataFromJson<T>(string pathToJson)
    {
        var allLines = await FileReader.ReadAllAsync(pathToJson);
        var entity = await Json.DeserializeAsync<Entity>(allLines);

        if (entity is null)
            throw new InvalidOperationException();

        var (_, dataJson) = EntityParser.ParseToStringTuple(entity);

        return await Json.DeserializeAsync<T>(dataJson, Options);
    }
}