using System.IO.Compression;
using Cake.Common.IO;
using Cake.Frosting;
using Octokit;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("GetAssets")]
public sealed class GetAssetsTask : AsyncFrostingTask<LaunchContext>
{
    public override async Task RunAsync(LaunchContext context)
    {
        var client = new GitHubClient(new ProductHeaderValue("DatafusLibrary"));

        var releases = await client.Repository.Release.GetAll("bot4dofus", "Datafus");
        var latestAsset = await client.Repository.Release.GetAllAssets("bot4dofus", "Datafus", releases.First().Id);

        var assetUri = new Uri(latestAsset.First().Url);
        var reqParams = new Dictionary<string, string>();
        var response = await client.Connection.Get<object>(assetUri, reqParams, "application/octet-stream");

        var tempPath = Path.GetTempPath();
        var destinationPath = tempPath + "datafusRelease";

        if (OperatingSystem.IsLinux())
            destinationPath = "/home/runner/work/_temp/datafusRelease";

        context.EnsureDirectoryDoesNotExist(destinationPath);

        if (context.FileExists(destinationPath + ".zip"))
            context.DeleteFile(destinationPath + ".zip");

        if (response.HttpResponse.Body is byte[] bodyBytes)
            await File.WriteAllBytesAsync($"{destinationPath}.zip", bodyBytes);

        ZipFile.ExtractToDirectory($"{destinationPath}.zip", destinationPath);
    }
}