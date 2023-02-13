using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.Extensions;
using DatafusLibrary.Core.Serialization;
using DatafusLibrary.LanguageModels.Sharp;
using DatafusLibrary.LanguageModels.Sharp.Descriptors;

namespace DatafusLibrary.Core.Parsers;

public static class EntityDefinitionParser
{
    public static async Task<List<BasicClass>> ParseToBasicClasses(string entityDefinitions)
    {
        var entityDefinitionsJson = await Json.DeserializeAsync<List<EntityType>>(entityDefinitions);

        List<BasicClass> basicClasses = new();

        if (entityDefinitionsJson is not null && entityDefinitionsJson.Any())
        {
            basicClasses.AddRange(entityDefinitionsJson
                .Select(entityDefinition => ParseToClassModel(entityDefinition, new BasicClass())));
        }

        return basicClasses;
    }

    private static BasicClass ParseToClassModel(EntityType entityDefinition, BasicClass classModel)
    {
        if (entityDefinition is null)
            throw new ArgumentNullException(nameof(entityDefinition));

        classModel.Namespace = entityDefinition.packageName ?? string.Empty;
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
                Name = name.ToPascalCase(),
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