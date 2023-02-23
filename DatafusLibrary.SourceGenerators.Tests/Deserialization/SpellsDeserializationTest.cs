using System.Runtime.Loader;
using com.ankamagames.dofus.datacenter.spells;
using DatafusLibrary.Core.Localization;
using DatafusLibrary.Core.Parsers;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests.Deserialization;

public class SpellsDeserializationTest
{
    private static ITestOutputHelper _output;
     private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
     private readonly string _entitiesBase = Path.GetFullPath(Path.Combine(DesktopPath, @".\Dofus2Botting\data\entities_json\"));

    public SpellsDeserializationTest(ITestOutputHelper output)
    {
        LoadAllReaderAssemblies(_entitiesBase);
        _output = output;
    }

    public static void LoadAllReaderAssemblies(string assemblyDir)
    {
      var assemblyFiles = Directory.EnumerateFiles(assemblyDir, "*.dll", SearchOption.AllDirectories);

        foreach (var assemblyFile in assemblyFiles)
        {
            try
            {
                AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFile);
            }
            catch (Exception ex)
            {
                _output.WriteLine(ex.Message);
            }
        }
    }

    [Fact]
    public async Task DeserializeSpells()
    {
        _output.WriteLine(string.Join(Environment.NewLine, "Spells deserialization test!"));

        var enTranslation = new TranslationLookup();
        await enTranslation.LoadTranslationFile("C:\\en.json");

        var pathToSpellsJson = _entitiesBase + "Spells.json";
        var pathToSpellStatesJson = _entitiesBase + "SpellStates.json";
        var pathToSpellTypesJson = _entitiesBase + "SpellTypes.json";
        var pathToSpellVariantsJson = _entitiesBase + "SpellVariants.json";
        var pathToSpellLevelsJson = _entitiesBase + "SpellLevels.json";

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