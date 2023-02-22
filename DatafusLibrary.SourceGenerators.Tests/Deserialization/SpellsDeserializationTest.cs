using com.ankamagames.dofus.datacenter.spells;
using DatafusLibrary.Core.Localization;
using DatafusLibrary.Core.Parsers;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests.Deserialization;

public class SpellsDeserializationTest
{
    private readonly ITestOutputHelper _output;

    public SpellsDeserializationTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task DeserializeSpells()
    {
        _output.WriteLine(string.Join(Environment.NewLine, "Spells deserialization test!"));

        var enTranslation = new TranslationLookup();
        await enTranslation.LoadTranslationFile("C:\\en.json");

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var entitiesBase = Path.GetFullPath(Path.Combine(desktopPath, @".\Dofus2Botting\data\entities_json\"));

        var pathToSpellsJson = entitiesBase + "Spells.json";
        var pathToSpellStatesJson = entitiesBase + "SpellStates.json";
        var pathToSpellTypesJson = entitiesBase + "SpellTypes.json";
        var pathToSpellVariantsJson = entitiesBase + "SpellVariants.json";
        var pathToSpellLevelsJson = entitiesBase + "SpellLevels.json";

        var spellData = await EntityDataParser.GetDataFromJson<List<Spell>>(pathToSpellsJson);
        var spellStateData = await EntityDataParser.GetDataFromJson<List<SpellState>>(pathToSpellStatesJson);
        var spellTypeData = await EntityDataParser.GetDataFromJson<List<SpellType>>(pathToSpellTypesJson);
        var spellVariantData = await EntityDataParser.GetDataFromJson<List<SpellVariant>>(pathToSpellVariantsJson);
        var spellLevelData = await EntityDataParser.GetDataFromJson<List<SpellLevel>>(pathToSpellLevelsJson);

        if (spellLevelData is not null)
        {
            var groupedById = spellLevelData.GroupBy(spellLevel => spellLevel.SpellId);
        }

        if (spellData is null || !spellData.Any())
            throw new InvalidOperationException("Failed to deserialize spell data!");

        foreach (var data in spellData)
        {
            var spellName = enTranslation.Get(data.NameId);

            if (string.IsNullOrEmpty(spellName))
            {
                _output.WriteLine($"Could not find en name for spellId: {data.Id}");
                continue;
            }

            if (spellTypeData is null || !spellTypeData.Any()) 
                continue;

            var spellType = spellTypeData.Select(spellType => spellType)
                .FirstOrDefault(spellType => spellType.Id.Equals(data.TypeId));
            if (spellType == null)
            {
                _output.WriteLine($"Could not find spellType for spellId: {data.Id}");
                continue;
            }

            var spellTypeName = enTranslation.Get(spellType.ShortNameId);
            if (spellTypeName is not null)
            {
                var replace = spellTypeName.Replace("\"", string.Empty).Replace(",", string.Empty);

                if (!string.IsNullOrEmpty(replace) || !replace.Equals(""))
                    _output.WriteLine($"SpellName: {spellName} SpellTypeName: {spellTypeName}");
            }
        }
    }
}