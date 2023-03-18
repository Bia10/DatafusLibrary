using DatafusLibrary.Core.DataDefinitions;
using DatafusLibrary.Core.LanguageModels.Sharp;
using DatafusLibrary.Core.LanguageModels.Sharp.Descriptors;
using DatafusLibrary.Core.Serialization;

namespace DatafusLibrary.Core.Parsers;

public class EntityDefinitionParser : IEntityDefinitionParser
{
    public async Task<IEnumerable<BasicClass>> ParseToClassModels(string entityDefinitions)
    {
        ArgumentException.ThrowIfNullOrEmpty(entityDefinitions);

        var entityDefinitionsJson = await Json.DeserializeAsync<IEnumerable<EntityType>>(entityDefinitions);
        if (entityDefinitionsJson is null)
            return Enumerable.Empty<BasicClass>();

        var entityTypeArray = entityDefinitionsJson as EntityType[] ?? entityDefinitionsJson.ToArray();

        return entityTypeArray.Any() ? entityTypeArray.Select(ParseToClassModel) : Enumerable.Empty<BasicClass>();
    }

    public BasicClass ParseToClassModel(EntityType entityDefinition)
    {
        ArgumentNullException.ThrowIfNull(entityDefinition);

        return new BasicClass
        {
            Namespace = entityDefinition.packageName ?? string.Empty,
            ClassName = entityDefinition.memberName ?? string.Empty,
            Properties = entityDefinition.fields is not null && entityDefinition.fields.Any()
                ? ParseFieldsToPropertyDescriptors(entityDefinition.fields)
                : Enumerable.Empty<PropertyDescriptor>()
        };
    }

    public PropertyDescriptor ParseFieldToPropertyDescriptor(Field encodedField)
    {
        ArgumentNullException.ThrowIfNull(encodedField);

        var fieldName = encodedField.name ?? string.Empty;

        return new PropertyDescriptor
        {
            Name = char.ToUpper(fieldName[0]) + fieldName[1..],
            Type = DecodeTypeValueToTypeStr(encodedField.type, fieldName, encodedField.vectorTypes)
        };
    }

    public IEnumerable<PropertyDescriptor> ParseFieldsToPropertyDescriptors(IEnumerable<Field> fields)
    {
        var entityFieldsArray = fields as Field[] ?? fields.ToArray();

        return entityFieldsArray.Select(ParseFieldToPropertyDescriptor);
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
        return DecodeValueType(fieldTypeValue) switch
        {
            EntityValueType.Reference => GetReferenceName(fieldTypeName),
            EntityValueType.Integer => "int",
            EntityValueType.Boolean => "bool",
            EntityValueType.String => "string",
            EntityValueType.Float => "float",
            EntityValueType.TranslationKey => "int",
            EntityValueType.UnsignedInteger => "uint",
            EntityValueType.Vector => GetVectorizedType(vectorType),
            _ => throw new ArgumentOutOfRangeException(nameof(fieldTypeValue),
                fieldTypeValue, "Unrecognized type value")
        };
    }

    public string FormatArgumentType(string argumentType)
    {
        if (argumentType.Contains("::", StringComparison.OrdinalIgnoreCase))
            argumentType = argumentType.Split("::", 2)[1];

        return argumentType
            .Replace("Number", "float", StringComparison.OrdinalIgnoreCase)
            .Replace("String", "string", StringComparison.Ordinal);
    }

    public string GetTwoDimVector(Field? vectorType)
    {
        ArgumentException.ThrowIfNullOrEmpty(vectorType?.name);

        var argumentType = vectorType.name
            .Replace("Vector.<Vector.<", string.Empty, StringComparison.Ordinal)
            .Replace(">>", string.Empty, StringComparison.OrdinalIgnoreCase);

        return $"List<List<{FormatArgumentType(argumentType)}>>";
    }

    public string GetOneDimVector(Field? vectorType)
    {
        ArgumentException.ThrowIfNullOrEmpty(vectorType?.name);

        var argumentType = vectorType.name
            .Replace("Vector.<", string.Empty, StringComparison.Ordinal)
            .Replace(">", string.Empty, StringComparison.OrdinalIgnoreCase);

        return $"List<{FormatArgumentType(argumentType)}>";
    }

    public string GetVectorizedType(Field? vectorType)
    {
        ArgumentException.ThrowIfNullOrEmpty(vectorType?.name);

        if (vectorType.name.StartsWith("Vector.<Vector.<", StringComparison.Ordinal))
            return GetTwoDimVector(vectorType);

        return vectorType.name.StartsWith("Vector.<", StringComparison.Ordinal)
            ? GetOneDimVector(vectorType)
            : string.Empty;
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