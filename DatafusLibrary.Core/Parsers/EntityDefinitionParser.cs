using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.Parsers.LanguageModels.Sharp;
using DatafusLibrary.Core.Parsers.LanguageModels.Sharp.Descriptors;

namespace DatafusLibrary.Core.Parsers;

public static class EntityDefinitionParser
{
    public static BasicClass ParseToClassModel(EntityType entityDefinition, BasicClass classModel)
    {
        if (entityDefinition is null)
        {
            throw new ArgumentNullException(nameof(entityDefinition));
        }

        classModel.ClassName = entityDefinition.memberName ?? string.Empty;

        if (entityDefinition.fields is not null && entityDefinition.fields.Any())
        {
            classModel.Properties = ParseProperties(entityDefinition.fields);
        }

        return classModel;
    }

    private static List<PropertyDescriptor> ParseProperties(List<Field> fields)
    {
        var properties = new List<PropertyDescriptor>();

        foreach (var field in fields)
        {
            var (name, type) = ParseField(field);

            var property = new PropertyDescriptor()
            {
                Name = name,
                Type = type,
            };

            properties.Add(property);
        }

        return properties;
    }

    private static (string name, string type) ParseField(Field encodedField)
    {
        (string name, string type) result = new()
        {
            name = encodedField.name ?? string.Empty,
            type = DecodeTypeValueToTypeStr(encodedField.type)
        };

        return result;
    }

    private static string DecodeTypeValueToTypeStr(int fieldTypeValue)
    {
        return fieldTypeValue switch
        {
            1 => "rectangle",
            -1 => "short",
            -2 => "bool",
            -3 => "string",
            -4 => "double",
            -5 => "int",
            // TODO: -99 complex type
            _ => string.Empty
        };
    }
}