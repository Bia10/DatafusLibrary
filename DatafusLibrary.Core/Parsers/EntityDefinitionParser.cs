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

    public static BasicClass ParseToClassModel(EntityType entityDefinition, BasicClass classModel)
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

    public static List<PropertyDescriptor> ParseProperties(List<Field> fields)
    {
        var properties = new List<PropertyDescriptor>();

        foreach (var field in fields)
        {
            var (name, type, vectorTypes) = ParseField(field);

            if (vectorTypes is not null)
            {
                var complexProperty = new PropertyDescriptor()
                {
                    Name = name.ToPascalCase(),
                    Type = DecodeTypeValueToTypeStr(vectorTypes.type, vectorTypes.name ?? string.Empty),
                };

                properties.Add(complexProperty);
                continue;
            }

            var property = new PropertyDescriptor()
            {
                Name = name.ToPascalCase(),
                Type = type,
            };

            properties.Add(property);
        }

        return properties;
    }

    public static (string name, string type, Field? vectorTypes) ParseField(Field encodedField)
    {
        (string name, string type, Field? vectorTypes) result = new()
        {
            name = encodedField.name ?? string.Empty,
            type = DecodeTypeValueToTypeStr(encodedField.type, encodedField.name ?? string.Empty),
            vectorTypes = encodedField.vectorTypes,
        };

        return result;
    }

    public static string DecodeTypeValueToTypeStr(int fieldTypeValue, string fieldTypeName)
    {
        switch (fieldTypeValue)
        {
            case 5: // table?
            case 4: // ??
            case 2: // reference to array of types?
            case 1: // reference to type
                return fieldTypeName;
            case -1:
                return "short";
            case -2:
                return "bool";
            case -3:
                return "string";
            case -4:
                return "double";
            case -5:
                return "int";
            case -6:
                return "uint";
            case -99:
            {
                return GetVectorizedTypeStr(fieldTypeName);
            }
            default:
            {
                Console.WriteLine($"Unrecognized value: {fieldTypeValue} of type name: {fieldTypeName}");
                return string.Empty;
            }
        }
    }

    private static string GetVectorizedTypeStr(string fieldTypeName)
    {
        var dataTypeStr = fieldTypeName
            .Replace("Vector.<", string.Empty)
            .Replace(">", string.Empty);

        return $"List<{dataTypeStr}>";
    }
}