using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.Extensions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.Serialization;
using DatafusLibrary.LanguageModels.Sharp;

namespace DatafusLibrary.Core.Parsers;

public static partial class EntityParser
{
    public static (string, string) ParseToStringTuple(Entity entityJson)
    {
        if (entityJson is null)
            throw new ArgumentNullException(nameof(entityJson));

        var defStringArray = entityJson.def?.ToStringArray();
        var dataStringArray = entityJson.data?.ToStringArray();

        if (defStringArray is not null && dataStringArray is not null)
        {
            var defString = string.Join(",", defStringArray);
            defString = defString.Insert(0, "[");
            defString = defString.Insert(defString.Length, "]");

            var dataString = string.Join(",", dataStringArray);
            dataString = dataString.Insert(0, "[");
            dataString = dataString.Insert(dataString.Length, "]");

            return (defString, dataString);
        }

        return (string.Empty, string.Empty);
    }

    public static async Task<List<BasicClass>> ParseEntityToBasicClass(string? pathToJson)
    {
        if (string.IsNullOrEmpty(pathToJson))
            throw new ArgumentNullException(nameof(pathToJson));

        var entityLines = await FileReader.ReadAllLinesAsync(pathToJson);

        var entity = await Json.DeserializeAsync<Entity>(string.Join(string.Empty, entityLines));

        if (entity is null)
            throw new InvalidOperationException();

        var (entityDefinitions, _) = ParseToStringTuple(entity);

        return await EntityDefinitionParser.ParseToBasicClasses(entityDefinitions);
    }

    public static async Task<List<List<EntityType>>> GetAllEntityTypesInDirectory(string pathToDir)
    {
        const string terminatorLine = "\t\"data\": [";
        var entityDefinitions = new List<List<EntityType>>();

        foreach (var fileName in Directory.GetFiles(pathToDir))
        {
            var linesUpToData = await FileReader.ReadAllLinesUntilLineAsync(fileName, Encoding.UTF8, terminatorLine);

            var joined = string.Join(string.Empty, linesUpToData);

            if (joined.EndsWith(','))
            {
                joined = MyRegex().Replace(joined, "}");
            }

            var entityType = await Json.DeserializeAsync<Entity>(joined);

            if (entityType is not null)
            {
                JsonSerializerOptions options = new()
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                var currentDefinition = JsonSerializer.Serialize(entityType.def, options);

                Console.WriteLine($"Group name: |{currentDefinition}| members count.");

                var entityDefinition = await Json.DeserializeAsync<List<EntityType>>(currentDefinition);

                if (entityDefinition is not null)
                {
                    entityDefinitions.Add(entityDefinition);
                }
            }
        }

        return entityDefinitions;
    }

    [System.Text.RegularExpressions.GeneratedRegex(",$")]
    private static partial System.Text.RegularExpressions.Regex MyRegex();
}