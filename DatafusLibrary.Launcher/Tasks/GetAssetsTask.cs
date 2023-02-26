using System.IO.Compression;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Frosting;
using Octokit;

namespace DatafusLibrary.Launcher.Tasks;

[TaskName("GetAssets")]
public sealed class GetAssetsTask : AsyncFrostingTask<LaunchContext>
{
    public override async Task RunAsync(LaunchContext context)
    {
        context.Information("GetAssets started...");

        var client = new GitHubClient(new ProductHeaderValue("DatafusLibrary"));

        var releases = await client.Repository.Release.GetAll("bot4dofus", "Datafus");
        var latestAsset = await client.Repository.Release.GetAllAssets("bot4dofus", "Datafus", releases[0].Id);

        var assetUri = new Uri(latestAsset[0].Url);
        var reqParams = new Dictionary<string, string>();
        var response = await client.Connection.Get<object>(assetUri, reqParams, "application/octet-stream");

        var downloadPath = context.LocalPathProvider.DatafusReleaseDownloadPath.FullPath;

        context.EnsureDirectoryDoesNotExist(downloadPath);

        if (context.FileExists(downloadPath + ".zip"))
            context.DeleteFile(downloadPath + ".zip");

        context.Information($"Saving assets to path: {downloadPath + ".zip"}");

        if (response.HttpResponse.Body is byte[] bodyBytes)
            await File.WriteAllBytesAsync($"{downloadPath}.zip", bodyBytes);

        context.Information($"Extracting assets to path: {downloadPath}");

        ZipFile.ExtractToDirectory($"{downloadPath}.zip", downloadPath);

        context.Information("GetAssets finished...");
    }
}