using DatafusLibrary.Core.Localization;
using DatafusLibrary.Core.Parsers;
using DatafusLibrary.LanguageModels.Sharp;

namespace DatafusLibrary.TestConsole;

internal static class Program
{
    private static async Task Main()
    {
        var translationLookUp = new TranslationLookup();

        await translationLookUp.LoadTranslationFile(
            "C:\\Users\\Bia\\Desktop\\Dofus2Botting\\data\\translations_json\\i18n_fr.json");

        translationLookUp.TryGetValue(1, out var translation);

        Console.WriteLine(translation);

        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory);
        var solutionDirectory = projectDirectory?.Parent?.Parent?.Parent;
        var path = solutionDirectory + @"\DatafusLibrary.TestConsole\MockData";

        var entities = await EntityParser.GetAllEntityTypesInDirectory(path);

        var flatList = entities.SelectMany(entityClass => entityClass).ToList();

        var listOfGroupingsByPackage = flatList
            .GroupBy(entityTep => entityTep.packageName)
            .Select(grouping => grouping)
            .ToList();

        var worldPackage = listOfGroupingsByPackage
            .Where(grouping => grouping.Key is not null && grouping.Key.EndsWith(".world"))
            .ToList();

        var baseClasses = new List<BasicClass>();

        foreach (var member in listOfGroupingsByPackage)
        {
            foreach (var entityOfGroup in member)
            {
                var listOfProps = EntityDefinitionParser.ParseProperties(entityOfGroup.fields);
                var baseClass = EntityDefinitionParser.ParseToClassModel(entityOfGroup, new BasicClass());

                baseClasses.Add(baseClass);

                foreach (var propertyDescriptor in listOfProps)
                {
                    Console.WriteLine($"Group name: |{propertyDescriptor.Name}| members count: |{propertyDescriptor.Type}|");
                }
            }
        }

        foreach (var basicClass in baseClasses)
        {
            Console.WriteLine($"ClassName: |{basicClass.ClassName}| namespace: |{basicClass.Namespace}|");
        }
    }
}