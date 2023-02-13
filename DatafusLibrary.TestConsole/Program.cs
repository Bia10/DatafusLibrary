using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.Extensions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.Core.Parsers.LanguageModels.Sharp;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.TestConsole;

internal static class Program
{
    public static (string, string) DefinitionsAndDataToString(Entity entityJson)
    {
        if (entityJson is null)
        {
            throw new ArgumentNullException(nameof(entityJson));
        }

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

    private static async Task Main()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory)?.Parent?.Parent?.FullName;

        var path = projectDirectory + @"\MockData\Areas.json";

        var areaLines = await FileReader.ReadAllLinesAsync(path);
        var areaEntity = await Json.DeserializeAsync<Entity>(string.Join(string.Empty, areaLines));

        var (entityDefinitions, entityData) = (string.Empty, string.Empty);
        if (areaEntity is not null)
        {
            (entityDefinitions, entityData) = DefinitionsAndDataToString(areaEntity);
        }

        var entityDefinitionsJson = await Json.DeserializeAsync<List<EntityType>>(entityDefinitions);

        List<BasicClass> basicClasses = new();

        if (entityDefinitionsJson is not null && entityDefinitionsJson.Any())
        {
            foreach (var entityDefinition in entityDefinitionsJson)
            {
                var parsedClass = EntityDefinitionParser.ParseToClassModel(entityDefinition, new BasicClass());

                basicClasses.Add(parsedClass);
            }
        }

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