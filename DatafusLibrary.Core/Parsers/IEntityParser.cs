using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;

namespace DatafusLibrary.Core.Parsers;

public interface IEntityParser
{
    Task<(string, string)> ParseToStringTupleAsync(Entity entityJson);

    Task<string> AggregateToStringAsync(IEnumerable<string> stringEnumerable);

    Task<string> GetEntityJsonAsync(string? pathToJson);

    Task<string> GetEntityDefinitionJsonAsync(string? pathToJson);

    Task<string> GetDefinitionStringAsync(string? pathToJson);

    Task<(string, string)> ParseToEntityDefDataTupleAsync(string? pathToJson);

    Task<IEnumerable<BasicClass>> GetAllBasicClassesFromDirAsync(string dirPath);

    Task<IEnumerable<EntityType>> GetAllEntityClassesAsync(string pathToDir);

    Task<IEnumerable<IGrouping<string?, EntityType>>> GetAllEntityClassesPackageGroupsAsync(string pathToDir);

    Task<IGrouping<string?, EntityType>> GetEntityClassesGroupsByPackageNamesAsync(string pathToDir, string packageName);

    IEnumerable<BasicClass> GetClassesFromPackageGroupAsync(IGrouping<string?, EntityType> packageGroup);
}