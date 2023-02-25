using System.IO.Compression;
using System.Text;
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

        var bodyStr = response.HttpResponse.Body.ToString();

        if (!string.IsNullOrEmpty(bodyStr))
        {
            var bytes = Encoding.ASCII.GetBytes(bodyStr);

            await File.WriteAllBytesAsync("datafusRelease.zip", bytes);

            var tempPath = Path.GetTempPath();

            if (string.IsNullOrEmpty(tempPath) && OperatingSystem.IsLinux())
                tempPath = "/home/runner/work/_temp/";

            ZipFile.ExtractToDirectory("datafusRelease.zip", tempPath + "datafusRelease");
        }
    }
}