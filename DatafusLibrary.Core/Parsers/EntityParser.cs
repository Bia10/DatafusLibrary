using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.Extensions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.Serialization;

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

    public static async Task<List<BasicClass>> GetAllBasicClassesFromDir(string dirPath)
    {
        var allBasicClasses = new List<BasicClass>();

        var allEntityClassesPackageGroups = await GetAllEntityClassesPackageGroups(dirPath);

        foreach (var basicClassesOfPackageGroup in allEntityClassesPackageGroups.Select(GetClassesFromPackageGroup))
            allBasicClasses.AddRange(basicClassesOfPackageGroup);

        return allBasicClasses;
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

    public static async Task<List<List<EntityType>>> GetAllEntityClassesInDirectory(string pathToDir)
    {
        const string terminatorLine = "\t\"data\": [";

        var entityDefinitions = new List<List<EntityType>>();

        foreach (var fileName in Directory.GetFiles(pathToDir))
        {
            var linesUpToData = await FileReader.ReadAllLinesAsync(fileName, terminatorLine);

            var joined = string.Join(string.Empty, linesUpToData);

            if (joined.EndsWith(','))
                joined = MyRegex().Replace(joined, "}");

            var entityType = await Json.DeserializeAsync<Entity>(joined);

            if (entityType is not null)
            {
                JsonSerializerOptions options = new()
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                var currentDefinition = JsonSerializer.Serialize(entityType.def, options);

                //Console.WriteLine($"Group name: |{currentDefinition}| members count.");

                var entityDefinition = await Json.DeserializeAsync<List<EntityType>>(currentDefinition);

                if (entityDefinition is not null)
                    entityDefinitions.Add(entityDefinition);
            }
        }

        return entityDefinitions;
    }

    public static async Task<List<EntityType>> GetAllEntityClasses(string pathToDir)
    {
        var entities = await GetAllEntityClassesInDirectory(pathToDir);

        var entityClasses = entities
            .SelectMany(entityClass => entityClass)
            .ToList();

        return entityClasses;
    }

    public static async Task<List<IGrouping<string?, EntityType>>> GetAllEntityClassesPackageGroups(string pathToDir)
    {
        var entityClasses = await GetAllEntityClasses(pathToDir);

        var listOfEntitiesGroupedByPackage = entityClasses
            .GroupBy(entityClass => entityClass.packageName)
            .Select(grouping => grouping)
            .Where(grouping => !string.IsNullOrEmpty(grouping.Key))
            .DistinctBy(grouping => grouping.Key)
            .OrderBy(grouping => grouping.Key)
            .ToList();

        return listOfEntitiesGroupedByPackage;
    }

    public static async Task<IGrouping<string?, EntityType>> GetEntityClassesGroupsByPackageNames(string pathToDir,
        string packageName)
    {
        var entityClasses = await GetAllEntityClasses(pathToDir);

        var entityClassesGroupedByPackageName = entityClasses
            .GroupBy(entityClass => entityClass.packageName)
            .Select(grouping => grouping)
            .Where(grouping => !string.IsNullOrEmpty(grouping.Key) &&
                               grouping.Key.Equals(packageName, StringComparison.Ordinal))
            .ToList()
            .First();

        return entityClassesGroupedByPackageName;
    }

    public static IEnumerable<BasicClass> GetClassesFromPackageGroup(IGrouping<string?, EntityType> packageGroup)
    {
        var baseClasses = new List<BasicClass>();

        foreach (var entityClass in packageGroup)
        {
            if (entityClass.fields is null)
            {
                Console.WriteLine($"Entity class with no fields encountered: {entityClass.memberName}");
                continue;
            }

            var baseClass = EntityDefinitionParser.ParseToClassModel(entityClass, new BasicClass());
            baseClasses.Add(baseClass);
        }

        return baseClasses;
    }

    [GeneratedRegex(",$")]
    private static partial Regex MyRegex();
}