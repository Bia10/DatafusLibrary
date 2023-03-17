using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.Extensions;
using DatafusLibrary.Core.IO;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.Core.Parsers;

public class EntityParser : IEntityParser
{
    private readonly IEntityDefinitionParser _entityDefinitionParser;

    public EntityParser(IEntityDefinitionParser entityDefinitionParser)
    {
        _entityDefinitionParser = entityDefinitionParser;
    }

    public async Task<(string, string)> ParseToStringTupleAsync(Entity entityJson)
    {
        ArgumentNullException.ThrowIfNull(entityJson);

        var defArray = entityJson.def?.ToStringArray();
        var dataArray = entityJson.data?.ToStringArray();

        ArgumentNullException.ThrowIfNull(defArray);
        ArgumentNullException.ThrowIfNull(dataArray);

        var defString = await AggregateToStringAsync(defArray);
        var dataString = await AggregateToStringAsync(dataArray);

        return (defString, dataString);
    }

    public async Task<string> AggregateToStringAsync(IEnumerable<string> stringEnumerable)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append('[');
        stringBuilder.Append(string.Join(',', stringEnumerable));
        stringBuilder.Append(']');

        return await Task.FromResult(stringBuilder.ToString());
    }

    public async Task<string> GetEntityJsonAsync(string? pathToJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToJson);

        return await FileReader.ReadAllAsync(pathToJson);
    }

    public async Task<string> GetEntityDefinitionJsonAsync(string? pathToJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToJson);

        return await FileReader.ReadAllAsync(pathToJson, "\t\"data\": [");
    }

    public async Task<Entity> GetEntityAsync(string? entityDefinitionJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityDefinitionJson);

        if (entityDefinitionJson.EndsWith(",", StringComparison.OrdinalIgnoreCase))
        {
            entityDefinitionJson = entityDefinitionJson.Remove(entityDefinitionJson.Length - 1, 1);
            entityDefinitionJson += '}';
        }

        var entityType = await Json.DeserializeAsync<Entity>(entityDefinitionJson);

        ArgumentNullException.ThrowIfNull(entityType);

        return entityType;
    }

    public async Task<IEnumerable<Entity>> GetAllEntityFromDirectoryAsync(string pathToDir)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToDir);

        var entityDefinitions = new List<Entity>();

        foreach (var fileName in Directory.EnumerateFiles(pathToDir))
        {
            var entityDefinitionJson = await GetEntityDefinitionJsonAsync(fileName);
            var entity = await GetEntityAsync(entityDefinitionJson);

            entityDefinitions.Add(entity);
        }

        return entityDefinitions;
    }

    public async Task<string> GetDefinitionStringAsync(string? pathToJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToJson);

        var entityDefinitionJson = await GetEntityDefinitionJsonAsync(pathToJson);
        var entity = await GetEntityAsync(entityDefinitionJson);

        var defArray = entity.def?.ToStringArray();

        ArgumentNullException.ThrowIfNull(defArray);

        return await AggregateToStringAsync(defArray);
    }

    public async Task<(string, string)> ParseToEntityDefDataTupleAsync(string? pathToJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToJson);

        var entityJson = await GetEntityJsonAsync(pathToJson);
        var entity = await Json.DeserializeAsync<Entity>(entityJson);

        return await ParseToStringTupleAsync(entity ?? throw new InvalidOperationException());
    }

    public IEnumerable<BasicClass> GetClassesFromPackageGroupAsync(IEnumerable<EntityType> packageGroup)
    {
        return (from entityType in packageGroup
                where entityType.fields is not null
                select _entityDefinitionParser.ParseToClassModel(entityType))
            .ToList();
    }

    public async Task<IEnumerable<BasicClass>> GetAllBasicClassesFromDirAsync(string dirPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(dirPath);

        var allBasicClasses = new List<BasicClass>();

        var allEntityClassesPackageGroups = await GetAllEntityTypesByPackageGroupsAsync(dirPath);

        Parallel.ForEach(allEntityClassesPackageGroups, packageGroup =>
        {
            var basicClassesOfPackageGroup = GetClassesFromPackageGroupAsync(packageGroup);
            lock (allBasicClasses)
            {
                allBasicClasses.AddRange(basicClassesOfPackageGroup);
            }
        });

        return allBasicClasses;
    }

    public async Task<IEnumerable<EntityType>> GetAllEntityTypesAsync(string pathToDir)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToDir);

        var entities = await GetAllEntityClassesInDirectoryAsync(pathToDir);

        return entities.SelectMany(entityClass => entityClass)
            .ToList();
    }

    public async Task<IEnumerable<IGrouping<string?, EntityType>>> GetAllEntityTypesByPackageGroupsAsync(string pathToDir)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToDir);

        var entityClasses = await GetAllEntityTypesAsync(pathToDir);

        var listOfEntitiesGroupedByPackage = entityClasses.GroupBy(entityClass => entityClass.packageName)
            .Select(grouping => grouping).Where(grouping => !string.IsNullOrEmpty(grouping.Key))
            .DistinctBy(grouping => grouping.Key).OrderBy(grouping => grouping.Key).ToList();

        return listOfEntitiesGroupedByPackage;
    }

    public async Task<IGrouping<string?, EntityType>> GetEntityTypesInGroupByPackageNameAsync(string pathToDir,
        string packageName)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToDir);

        var entityClasses = await GetAllEntityTypesAsync(pathToDir);

        var entityClassesGroupedByPackageName = entityClasses.GroupBy(entityClass => entityClass.packageName)
            .Select(grouping => grouping)
            .Where(grouping => !string.IsNullOrEmpty(grouping.Key) && grouping.Key.Equals(packageName, StringComparison.Ordinal))
            .ToList().First();

        return entityClassesGroupedByPackageName;
    }

    public async Task<List<IEnumerable<EntityType>>> GetAllEntityClassesInDirectoryAsync(string pathToDir)
    {
        ArgumentException.ThrowIfNullOrEmpty(pathToDir);

        var entityDefinitions = new List<IEnumerable<EntityType>>();

        foreach (var fileName in Directory.EnumerateFiles(pathToDir))
        {
            var entityDefinitionJson = await GetEntityDefinitionJsonAsync(fileName);
            var entity = await GetEntityAsync(entityDefinitionJson);

            var currentDefinition = JsonSerializer.Serialize(entity.def, new JsonSerializerOptions()
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });

            //Console.WriteLine($"Group name: |{currentDefinition}| members count.");

            var entityDefinition = await Json.DeserializeAsync<IEnumerable<EntityType>>(currentDefinition);

            if (entityDefinition is not null)
                entityDefinitions.Add(entityDefinition);
        }

        return entityDefinitions;
    }
}