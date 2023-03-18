using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;

namespace DatafusLibrary.Core.Parsers;

public interface IEntityDefinitionParser
{
    string GetReferenceName(string fieldTypeName);

    string GetVectorizedType(Field? vectorType);

    string DecodeTypeValueToTypeStr(int fieldTypeValue, string fieldTypeName, Field? vectorType);

    string FormatArgumentType(string argumentType);

    string GetOneDimVector(Field? vectorType);

    string GetTwoDimVector(Field? vectorType);

    EntityValueType DecodeValueType(int fieldTypeValue);

    PropertyDescriptor ParseFieldToPropertyDescriptor(Field encodedField);

    IEnumerable<PropertyDescriptor> ParseFieldsToPropertyDescriptors(IEnumerable<Field> fields);

    BasicClass ParseToClassModel(EntityType entityDefinition);

    Task<IEnumerable<BasicClass>> ParseToClassModels(string entityDefinitions);
}