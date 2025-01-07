namespace Digdir.Library.Dialogporten.WebApiClient.Integration.Tests;

public sealed class RefitterInterfaceTests
{

    [Fact]
    public async Task FailIfRefitterInterfaceDoesNotMatch()
    {
        var rootPath = GetSolutionRootFolder();
        var webApiClientPath = Path.Combine(rootPath!, "src/Digdir.Library.Dialogporten.WebApiClient/Features/V1");
        var currentDirectory = Path.Combine(rootPath!, "tests/Digdir.Library.Dialogporten.WebApiClient.Integration.Tests");
        var newRifitterPath = Path.Combine(currentDirectory, "refitter/RefitterInterface.cs");
        var newRefitter = File.ReadAllText(newRifitterPath);
        Assert.True(File.Exists(newRifitterPath));
        await Verify(newRefitter, extension: "cs")
            .UseFileName("RefitterInterface")
            .UseDirectory(webApiClientPath);

        var path = Path.Combine(webApiClientPath, "RefitterInterface.received.cs");
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        Assert.False(File.Exists(path));
    }
    private static string? GetSolutionRootFolder()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var solutionFolder = currentDirectory;
        while (solutionFolder != null && Directory.GetFiles(solutionFolder, "*.sln").Length == 0)
        {
            solutionFolder = Directory.GetParent(solutionFolder)?.FullName;
        }
        return solutionFolder;
    }

}
