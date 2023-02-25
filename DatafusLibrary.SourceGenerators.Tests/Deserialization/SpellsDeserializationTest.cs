using com.ankamagames.dofus.datacenter.spells;
using DatafusLibrary.Core.Localization;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.SourceGenerators.Tests.Helpers.NLog;
using NLog;
using NLog.Config;
using Xunit;
using Xunit.Abstractions;

namespace DatafusLibrary.SourceGenerators.Tests.Deserialization;

public class SpellsDeserializationTest
{
    private static ILogger? _logger;
    private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    private readonly string _entitiesBase =
        Path.GetFullPath(Path.Combine(DesktopPath, @".\Dofus2Botting\data\entities_json\"));

    public SpellsDeserializationTest(ITestOutputHelper? iTestOutputHelper)
    {
        var logFactory = new LogFactory();
        logFactory.ThrowExceptions = true;
        var configuration = new LoggingConfiguration();
        var testOutputTarget = new TestOutputTarget();

        testOutputTarget.Add(iTestOutputHelper, nameof(SpellsDeserializationTest));
        configuration.AddRuleForAllLevels(testOutputTarget, nameof(SpellsDeserializationTest));
        logFactory.Configuration = configuration;

        _logger = logFactory.GetLogger(nameof(SpellsDeserializationTest));
    }


    [Fact]
    public async Task DeserializeSpells()
    {
        if (_logger is null)
            throw new NullReferenceException(nameof(_logger));

        _logger.Info(string.Join(Environment.NewLine, $"Spells deserialization started at: {DateTime.Now}"));

        var enTranslation = new TranslationLookup();

        var tempPath = Path.GetTempPath();
        var pathToTranslationFile = tempPath + "datafusRelease\\data\\translations_json\\i18n_en.json";

        if (OperatingSystem.IsLinux())
            pathToTranslationFile = tempPath + "datafusRelease\\data\\translations_json\\i18n_en.json"
                .Replace('\\', '/');

        await enTranslation.LoadTranslationFile(pathToTranslationFile);

        var pathToSpellsJson = _entitiesBase + "Spells.json";
        var pathToSpellStatesJson = _entitiesBase + "SpellStates.json";
        var pathToSpellTypesJson = _entitiesBase + "SpellTypes.json";
        var pathToSpellVariantsJson = _entitiesBase + "SpellVariants.json";
        var pathToSpellLevelsJson = _entitiesBase + "SpellLevels.json";

        var spellData = await EntityDataParser.GetDataFromJson<List<Spell>>(pathToSpellsJson);
        _logger.Info(string.Join(Environment.NewLine, $"Deserialized {spellData?.Count} spellData!"));

        var spellStateData = await EntityDataParser.GetDataFromJson<List<SpellState>>(pathToSpellStatesJson);
        _logger.Info(string.Join(Environment.NewLine, $"Deserialized {spellStateData?.Count} spellStateData!"));

        var spellTypeData = await EntityDataParser.GetDataFromJson<List<SpellType>>(pathToSpellTypesJson);
        _logger.Info(string.Join(Environment.NewLine, $"Deserialized {spellTypeData?.Count} spellTypeData!"));

        var spellVariantData = await EntityDataParser.GetDataFromJson<List<SpellVariant>>(pathToSpellVariantsJson);
        _logger.Info(string.Join(Environment.NewLine, $"Deserialized {spellVariantData?.Count} spellVariantData!"));

        var spellLevelData = await EntityDataParser.GetDataFromJson<List<SpellLevel>>(pathToSpellLevelsJson);
        _logger.Info(string.Join(Environment.NewLine, $"Deserialized {spellLevelData?.Count} spellLevelData!"));


        _logger.Info(string.Join(Environment.NewLine, $"Deserialization finished at: {DateTime.Now}"));

        var groupedById = spellLevelData?.GroupBy(spellLevel => spellLevel.SpellId);
        _logger.Info($"SpellLevel groups by spellID: {groupedById?.Count()}");

        if (spellData is null || !spellData.Any())
            throw new InvalidOperationException("Failed to deserialize spell data!");

        foreach (var data in spellData)
        {
            var spellName = enTranslation.Get(data.NameId);

            if (string.IsNullOrEmpty(spellName))
            {
                _logger.Info($"Could not find en name for spellId: {data.Id}");
                continue;
            }

            if (spellTypeData is null || !spellTypeData.Any())
                continue;

            var spellType = spellTypeData.Select(spellType => spellType)
                .FirstOrDefault(spellType => spellType.Id.Equals(data.TypeId));
            if (spellType == null)
            {
                _logger.Info($"Could not find spellType for spellId: {data.Id}");
                continue;
            }

            var spellTypeName = enTranslation.Get(spellType.ShortNameId);
            if (spellTypeName is not null)
            {
                var replace = spellTypeName.Replace("\"", string.Empty).Replace(",", string.Empty);

                if (!string.IsNullOrEmpty(replace) || !replace.Equals(""))
                    _logger.Info($"SpellName: {spellName} SpellTypeName: {spellTypeName}");
            }
        }
    }
}