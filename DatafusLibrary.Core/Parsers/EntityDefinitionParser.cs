using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.Core.Parsers;

public class EntityDefinitionParser : IEntityDefinitionParser
{
    public async Task<List<BasicClass>> ParseToBasicClasses(string entityDefinitions)
    {
        var entityDefinitionsJson = await Json.DeserializeAsync<List<EntityType>>(entityDefinitions);

        List<BasicClass> basicClasses = new();

        if (entityDefinitionsJson is not null && entityDefinitionsJson.Any())
            basicClasses.AddRange(entityDefinitionsJson.Select(entityDefinition
                => ParseToClassModel(entityDefinition, new BasicClass())));

        return basicClasses;
    }

    public BasicClass ParseToClassModel(EntityType entityDefinition, BasicClass classModel)
    {
        ArgumentNullException.ThrowIfNull(entityDefinition);

        classModel.Namespace = entityDefinition.packageName ?? string.Empty;
        classModel.ClassName = entityDefinition.memberName ?? string.Empty;

        if (entityDefinition.fields is not null && entityDefinition.fields.Any())
            classModel.Properties = ParseProperties(entityDefinition.fields);

        return classModel;
    }

    public List<PropertyDescriptor> ParseProperties(List<Field> fields)
    {
        var properties = new List<PropertyDescriptor>();

        foreach (var field in fields)
        {
            var (name, type, _) = ParseField(field);

            var property = new PropertyDescriptor
            {
                Name = char.ToUpper(name[0]) + name[1..],
                Type = type
            };

            properties.Add(property);
        }

        return properties;
    }

    public EntityValueType DecodeValueType(int fieldTypeValue)
    {
        return fieldTypeValue switch
        {
            1 or 2 or 3 or 4 or 5 => EntityValueType.Reference,
            -1 => EntityValueType.Integer,
            -2 => EntityValueType.Boolean,
            -3 => EntityValueType.String,
            -4 => EntityValueType.Float,
            -5 => EntityValueType.TranslationKey,
            -6 => EntityValueType.UnsignedInteger,
            -99 => EntityValueType.Vector,
            _ => throw new ArgumentOutOfRangeException(nameof(fieldTypeValue),
                fieldTypeValue,
                "Unrecognized type value")
        };
    }

    public string DecodeTypeValueToTypeStr(int fieldTypeValue, string fieldTypeName, Field? vectorType)
    {
        var decodedType = DecodeValueType(fieldTypeValue);

        var fieldType = decodedType switch
        {
            EntityValueType.Reference => GetReferenceName(fieldTypeName),
            EntityValueType.Integer => "int",
            EntityValueType.Boolean => "bool",
            EntityValueType.String => "string",
            EntityValueType.Float => "float",
            EntityValueType.TranslationKey => "int",
            EntityValueType.UnsignedInteger => "uint",
            EntityValueType.Vector => GetVectorizedType(fieldTypeValue, vectorType),
            _ => throw new ArgumentOutOfRangeException()
        };

        return fieldType;
    }

    public string GetVectorizedType(int fieldTypeValue, Field? vectorType)
    {
        if (string.IsNullOrEmpty(vectorType?.name))
            throw new ArgumentNullException(nameof(vectorType));

        if (vectorType.name.StartsWith("Vector.<Vector.<", StringComparison.Ordinal))
        {
            var argumentType = vectorType.name.Replace("Vector.<Vector.<", string.Empty).Replace(">>", string.Empty);

            if (argumentType.Contains("::"))
                argumentType = argumentType.Split("::", 2)[1];

            if (argumentType.Equals("Number", StringComparison.OrdinalIgnoreCase))
                argumentType = "float";

            return $"List<List<{argumentType}>>";
        }

        if (vectorType.name.StartsWith("Vector.<", StringComparison.Ordinal))
        {
            var argumentType = vectorType.name.Replace("Vector.<", string.Empty).Replace(">", string.Empty);

            if (argumentType.Contains("::"))
                argumentType = argumentType.Split("::", 2)[1];

            if (argumentType.Equals("Number", StringComparison.OrdinalIgnoreCase))
                argumentType = "float";

            if (argumentType.Equals("String"))
                argumentType = "string";

            return $"List<{argumentType}>";
        }

        if (fieldTypeValue.Equals(-99) && vectorType.name.StartsWith("Vector.<", StringComparison.Ordinal))
        {
            var argumentType = vectorType.name.Replace("Vector.<", string.Empty).Replace(">", string.Empty);

            if (argumentType.Contains("::"))
                argumentType = argumentType.Split("::", 2)[1];

            if (argumentType.Equals("Number", StringComparison.OrdinalIgnoreCase))
                argumentType = "float";

            if (argumentType.Equals("String"))
                argumentType = "string";

            return $"List<{argumentType}>";
        }

        return string.Empty;
    }

    public string GetReferenceName(string fieldTypeName)
    {
        return fieldTypeName switch
        {
            "bonusCharacteristics" => "MonsterBonusCharacteristics",
            "parameters" => "QuestObjectiveParameters",
            "coords" => "Point",
            "bounds" => "Rectangle",
            _ => throw new ArgumentOutOfRangeException(nameof(fieldTypeName),
                fieldTypeName,
                "Unrecognized reference name")
        };
    }

    public (string name, string type, Field? vectorTypes) ParseField(Field encodedField)
    {
        (string name, string type, Field? vectorTypes) result = new()
        {
            name = encodedField.name ?? string.Empty,
            type = DecodeTypeValueToTypeStr(encodedField.type,
                encodedField.name ?? string.Empty,
                encodedField.vectorTypes),
            vectorTypes = encodedField.vectorTypes
        };

        return result;
    }
}