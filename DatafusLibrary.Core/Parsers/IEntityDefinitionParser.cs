using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;

namespace DatafusLibrary.Core.Parsers;

public interface IEntityDefinitionParser
{
    string GetReferenceName(string fieldTypeName);

    string GetVectorizedType(Field? vectorType);

    string DecodeTypeValueToTypeStr(int fieldTypeValue, string fieldTypeName, Field? vectorType);

    EntityValueType DecodeValueType(int fieldTypeValue);

    IEnumerable<PropertyDescriptor> ParseProperties(IEnumerable<Field> fields);

    BasicClass ParseToClassModel(EntityType entityDefinition);

    Task<List<BasicClass>> ParseToBasicClasses(string entityDefinitions);
}