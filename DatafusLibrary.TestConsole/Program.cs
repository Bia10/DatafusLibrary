using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.TestConsole;

internal static class Program
{
    private static async Task Main()
    {
        var entityFileLines = await FileReader.ReadAllLinesAsync(null);
        var entityType = await Json.DeserializeAsync<Entity>(string.Join(string.Empty, entityFileLines));

        if (entityType is not null)
        {
            var (entityDefinitions, _) = EntityParser.ParseToStringTuple(entityType);
            var basicClasses = await EntityDefinitionParser.ParseToBasicClasses(entityDefinitions);
        }
    }
}