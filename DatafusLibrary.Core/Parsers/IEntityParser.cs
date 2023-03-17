using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;

namespace DatafusLibrary.Core.Parsers;

public interface IEntityParser
{
    Task<(string, string)> ParseToStringTupleAsync(Entity entityJson);

    Task<string> AggregateToStringAsync(IEnumerable<string> stringEnumerable);

    Task<string> GetEntityJsonAsync(string? pathToJson);

    Task<string> GetEntityDefinitionJsonAsync(string? pathToJson);

    Task<Entity> GetEntityAsync(string? entityDefinitionJson);

    Task<IEnumerable<Entity>> GetAllEntityFromDirectoryAsync(string pathToDir);

    Task<string> GetDefinitionStringAsync(string? pathToJson);

    Task<(string, string)> ParseToEntityDefDataTupleAsync(string? pathToJson);

    IEnumerable<BasicClass> GetClassesFromPackageGroupAsync(IEnumerable<EntityType> packageGroup);

    Task<IEnumerable<BasicClass>> GetAllBasicClassesFromDirAsync(string dirPath);

    Task<IEnumerable<EntityType>> GetAllEntityTypesAsync(string pathToDir);

    Task<IEnumerable<IGrouping<string?, EntityType>>> GetAllEntityTypesByPackageGroupsAsync(string pathToDir);

    Task<IGrouping<string?, EntityType>> GetEntityTypesInGroupByPackageNameAsync(string pathToDir, string packageName);

    Task<List<IEnumerable<EntityType>>> GetAllEntityClassesInDirectoryAsync(string pathToDir);
}