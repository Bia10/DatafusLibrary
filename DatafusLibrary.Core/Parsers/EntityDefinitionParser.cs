using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.Core.Parsers;

public class EntityDefinitionParser : IEntityDefinitionParser
{
    public async Task<List<BasicClass>> ParseToBasicClasses(string entityDefinitions)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityDefinitions);

        var entityDefinitionsJson = await Json.DeserializeAsync<List<EntityType>>(entityDefinitions);

        List<BasicClass> basicClasses = new();

        if (entityDefinitionsJson is not null && entityDefinitionsJson.Any())
            basicClasses.AddRange(entityDefinitionsJson.Select(ParseToClassModel));

        return basicClasses;
    }

    public BasicClass ParseToClassModel(EntityType entityDefinition)
    {
        var classModel = new BasicClass();

        ArgumentNullException.ThrowIfNull(entityDefinition);

        classModel.Namespace = entityDefinition.packageName ?? string.Empty;
        classModel.ClassName = entityDefinition.memberName ?? string.Empty;

        if (entityDefinition.fields is not null && entityDefinition.fields.Any())
            classModel.Properties = ParseProperties(entityDefinition.fields);

        return classModel;
    }

    public (string name, string type, Field? vectorTypes) ParseField(Field encodedField)
    {
        ArgumentNullException.ThrowIfNull(encodedField);

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

    public IEnumerable<PropertyDescriptor> ParseProperties(IEnumerable<Field> fields)
    {
        var entityFieldsArray = fields as Field[] ?? fields.ToArray();
        var properties = new List<PropertyDescriptor>(entityFieldsArray.Length);

        foreach (var field in entityFieldsArray)
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
            >= 1 and <= 5 => EntityValueType.Reference,
            -1 => EntityValueType.Integer,
            -2 => EntityValueType.Boolean,
            -3 => EntityValueType.String,
            -4 => EntityValueType.Float,
            -5 => EntityValueType.TranslationKey,
            -6 => EntityValueType.UnsignedInteger,
            -99 => EntityValueType.Vector,
            _ => throw new ArgumentOutOfRangeException(nameof(fieldTypeValue),
                fieldTypeValue, "Unrecognized type value")
        };
    }

    public string DecodeTypeValueToTypeStr(int fieldTypeValue, string fieldTypeName, Field? vectorType)
    {
        var fieldType = DecodeValueType(fieldTypeValue) switch
        {
            EntityValueType.Reference => GetReferenceName(fieldTypeName),
            EntityValueType.Integer => "int",
            EntityValueType.Boolean => "bool",
            EntityValueType.String => "string",
            EntityValueType.Float => "float",
            EntityValueType.TranslationKey => "int",
            EntityValueType.UnsignedInteger => "uint",
            EntityValueType.Vector => GetVectorizedType(vectorType),
            _ => throw new ArgumentOutOfRangeException()
        };

        return fieldType;
    }

    private static string FormatArgumentType(string argumentType)
    {
        if (argumentType.Contains("::", StringComparison.Ordinal))
            argumentType = argumentType.Split("::", 2)[1];

        if (argumentType.Equals("Number", StringComparison.OrdinalIgnoreCase))
            argumentType = "float";

        if (argumentType.Equals("String", StringComparison.Ordinal))
            argumentType = "string";

        return argumentType;
    }

    public static string GetTwoDimVector(Field? vectorType)
    {
        ArgumentException.ThrowIfNullOrEmpty(vectorType?.name);

        var argumentType = vectorType.name
            .Replace("Vector.<Vector.<", string.Empty, StringComparison.Ordinal)
            .Replace(">>", string.Empty, StringComparison.Ordinal);

        argumentType = FormatArgumentType(argumentType);

        return $"List<List<{argumentType}>>";
    }

    public static string GetOneDimVector(Field? vectorType)
    {
        ArgumentException.ThrowIfNullOrEmpty(vectorType?.name);

        var argumentType = vectorType.name
            .Replace("Vector.<", string.Empty, StringComparison.Ordinal)
            .Replace(">", string.Empty, StringComparison.Ordinal);

        argumentType = FormatArgumentType(argumentType);

        return $"List<{argumentType}>";
    }

    public string GetVectorizedType(Field? vectorType)
    {
        ArgumentException.ThrowIfNullOrEmpty(vectorType?.name);

        if (vectorType.name.StartsWith("Vector.<Vector.<", StringComparison.Ordinal))
            return GetTwoDimVector(vectorType);

        if (vectorType.name.StartsWith("Vector.<", StringComparison.Ordinal))
            return GetOneDimVector(vectorType);

        return string.Empty;
    }

    public string GetReferenceName(string fieldTypeName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldTypeName);

        return fieldTypeName switch
        {
            "bonusCharacteristics" => "MonsterBonusCharacteristics",
            "parameters" => "QuestObjectiveParameters",
            "coords" => "Point",
            "bounds" => "Rectangle",
            _ => throw new ArgumentOutOfRangeException(nameof(fieldTypeName),
                fieldTypeName, "Unrecognized reference name")
        };
    }
}