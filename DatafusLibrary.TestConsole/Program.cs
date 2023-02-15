using DatafusLibrary.Core.Parsers;

namespace DatafusLibrary.TestConsole;

internal static class Program
{
    private static async Task Main()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var projectDirectory = Directory.GetParent(workingDirectory);
        var solutionDirectory = projectDirectory?.Parent?.Parent?.Parent;
        var path = solutionDirectory + @"\DatafusLibrary.TestConsole\MockData";

        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var dirPath = Path.GetFullPath(Path.Combine(desktopPath, @".\Dofus2Botting\data\entities_json"));

        const string packageName = "com.ankamagames.dofus.datacenter.world";
        var worldPackage = await EntityParser.GetEntityClassesPackageGroupByName(dirPath, packageName);
        var groupClassesFromPackageGroup = EntityParser.GetClassesFromPackageGroup(worldPackage);
    }
}