using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;
using DatafusLibrary.Core.Serialization;

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
            Console.WriteLine(
                $"getting properties for class: {classModel.Namespace} in package: {classModel.ClassName} ");
            classModel.Properties = ParseProperties(entityDefinition.fields);
        }

        return classModel;
    }

    public static List<PropertyDescriptor> ParseProperties(List<Field> fields)
    {
        var properties = new List<PropertyDescriptor>();

        foreach (var field in fields)
        {
            var (name, type, _) = ParseField(field);

            var property = new PropertyDescriptor()
            {
                Name = char.ToUpper(name[0]) + name[1..],
                Type = type
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
            type = DecodeTypeValueToTypeStr(encodedField.type, encodedField.name ?? string.Empty, encodedField.vectorTypes),
            vectorTypes = encodedField.vectorTypes
        };

        return result;
    }

    public static string DecodeTypeValueToTypeStr(int fieldTypeValue, string fieldTypeName, Field? vectorType)
    {
        var fieldType = fieldTypeValue switch
        {
            -1 => "int",
            -2 => "bool",
            -3 => "string",
            -4 => "float",
            -5 => "int",
            -6 => "uint",
            -99 => GetVectorizedType(fieldTypeValue, vectorType),
            _ => GetReferenceName(fieldTypeName)
        };

        if (string.IsNullOrEmpty(fieldType))
        {
            Console.WriteLine($"Unrecognized value: {fieldTypeValue} of type name: {fieldTypeName}");
        }

        return fieldType;
    }

    public static string GetVectorizedType(int fieldTypeValue, Field? vectorType)
    {
        if (string.IsNullOrEmpty(vectorType?.name))
        {
            throw new ArgumentNullException(nameof(vectorType));
        }

        if (vectorType.name.StartsWith("Vector.<Vector.<"))
        {
            var argumentType = vectorType.name
                .Replace("Vector.<Vector.<", string.Empty)
                .Replace(">>", string.Empty);

            if (argumentType.Contains("::"))
            {
                argumentType = argumentType.Split("::", 2)[1];
            }

            if (argumentType.Equals("Number", StringComparison.OrdinalIgnoreCase))
            {
                argumentType = "float";
            }

            return $"List<List<{argumentType}>>";
        }

        if (vectorType.name.StartsWith("Vector.<"))
        {
            var argumentType = vectorType.name
                .Replace("Vector.<", string.Empty)
                .Replace(">", string.Empty);

            if (argumentType.Contains("::"))
            {
                argumentType = argumentType.Split("::", 2)[1];
            }

            if (argumentType.Equals("Number", StringComparison.OrdinalIgnoreCase))
            {
                argumentType = "float";
            }

            if (argumentType.Equals("String"))
            {
                argumentType = "string";
            }

            return $"List<{argumentType}>";
        }

        if (fieldTypeValue.Equals(-99) && vectorType.name.StartsWith("Vector.<"))
        {
            var argumentType = vectorType.name
                .Replace("Vector.<", string.Empty)
                .Replace(">", string.Empty);

            if (argumentType.Contains("::"))
            {
                argumentType = argumentType.Split("::", 2)[1];
            }

            if (argumentType.Equals("Number", StringComparison.OrdinalIgnoreCase))
            {
                argumentType = "float";
            }

            if (argumentType.Equals("String"))
            {
                argumentType = "string";
            }

            return $"List<{argumentType}>";
        }
            
        return string.Empty;
    }

    public static string GetReferenceName(string fieldTypeName)
    {
        Console.WriteLine($"Unrecognized typeValue of type name: {fieldTypeName}");

        if (fieldTypeName.Equals("bonusCharacteristics"))
        {
            return "MonsterBonusCharacteristics";
        }

        if (fieldTypeName.Equals("parameters"))
        {
            return "QuestObjectiveParameters";
        }

        if (fieldTypeName.Equals("coords"))
        {
            return "Point";
        }

        if (fieldTypeName.Equals("bounds"))
        {
            return "Rectangle";
        }

        return string.Empty;
    }
}