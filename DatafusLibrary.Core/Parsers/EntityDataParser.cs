using System.Text.Json;
using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.Core.Parsers;

public class EntityDataParser : IEntityDataParser
{
    private readonly IEntityParser _entityParser;

    private readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public EntityDataParser(IEntityParser entityParser)
    {
        _entityParser = entityParser;
    }

    public async Task<T?> GetDataFromJsonAsync<T>(string pathToJson)
    {
        var allLines = await FileReader.ReadAllAsync(pathToJson);
        var entity = await Json.DeserializeAsync<Entity>(allLines);

        if (entity is null)
            throw new InvalidOperationException();

        var (_, dataJson) = await _entityParser.ParseToStringTupleAsync(entity);

        return await Json.DeserializeAsync<T>(dataJson, Options);
    }
}