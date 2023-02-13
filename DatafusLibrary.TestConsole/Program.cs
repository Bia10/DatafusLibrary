using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.TestConsole;

internal static class Program
{
    private static async Task Main()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;

        var path = projectDirectory + @"\MockData\Areas.json";

        var areaLines = await FileReader.ReadAllLinesAsync(path);
        var areaEntity = await Json.DeserializeAsync<Entity>(string.Join(string.Empty, areaLines));

        if (areaEntity is not null)
        {
            var (entityDefinitions, _) = EntityParser.ParseToStringTuple(areaEntity);

            var basicClasses = await EntityDefinitionParser.ParseToBasicClasses(entityDefinitions);

            foreach (var basicClass in basicClasses)
            {
                Console.Write($"ClassName: {basicClass.ClassName} \n");

                foreach (var propertyDescriptor in basicClass.Properties)
                {
                    Console.Write($"PropertyName: {propertyDescriptor.Name} PropertyType: {propertyDescriptor.Type} \n" );
                }
            }
        }
    }
}