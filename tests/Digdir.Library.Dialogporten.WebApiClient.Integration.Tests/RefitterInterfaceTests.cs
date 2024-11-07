namespace Digdir.Library.Dialogporten.WebApiClient.Integration.Tests;

public sealed class RefitterInterfaceTests : IDisposable
{

    [Fact]
    public async Task FailIfRefitterInterfaceDoesNotMatch()
    {
        // Amund: prøv mæ mocka filesystem. det e litt for kult te å ikk prøve vartfall.
        // ser ut som det er vrient å bruke verify sammen med mocka filesystem
        // Constructor, dispose. Ser ut som å være en decent måte å takle cleanup på. Funker ikke når testen feiler så dør processen.
        // Async dispose ikke lagt til enda. kommer i v3. som nå er i alpha/beta enda
        // men AsyncLifetime er i V2.
        var rootPath = GetSolutionRootFolder();
        var webApiClientPath = Path.Combine(rootPath!, "src/Digdir.Library.Dialogporten.WebApiClient/Features/V1");
        var currentDirectory = Path.Combine(rootPath!, "tests/Digdir.Library.Dialogporten.WebApiClient.Integration.Tests");
        var newRifitterPath = Path.Combine(currentDirectory, "refitter/RefitterInterface.cs");
        var newRefitter = File.ReadAllText(newRifitterPath);
        // Den feiler og viser diff samtidig. testene er av definisjon ferdig når diff vises. er det noe måte å unngå dette på?
        // Den lager bare fil om den feiler. er det noe som kan skrus av?
        // Om den feiler ved en diff så må den kjøres på nytt uansett. det e egt litt skuffed. men jaja.
        // Træng i da egt å fjerne fila eller feil?
        // treng da egt bære å fjerne om den finnes fra en gammel runde. altså slett før verify
        Assert.True(File.Exists(newRifitterPath));
        await Verify(newRefitter, extension: "cs")
            .UseFileName("RefitterInterface")
            .UseDirectory(webApiClientPath)
            .OnVerify(
                before: () => { },
                after: () =>
                {
                    // Funke ikkje! DEN FÆILE OG ÅPNE DIFF!
                    // var path = Path.Combine(webApiClientPath, "RefitterInterface.received.cs");
                    // if (File.Exists(path))
                    // {
                    //     File.Delete(path);
                    // }
                    // if (File.Exists(newRifitterPath))
                    // {
                    //     File.Delete(newRifitterPath);
                    // }
                    // Assert.False(File.Exists(newRifitterPath));
                });
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

    public void Dispose()
    {
        // var rootPath = GetSolutionRootFolder();
        // var webApiClientPath = Path.Combine(rootPath!, "src/Digdir.Library.Dialogporten.WebApiClient/Features/V1");
        // var path = Path.Combine(webApiClientPath, "RefitterInterface.received.cs");
        // if (File.Exists(path))
        // {
        //     File.Delete(path);
        // }
    }
}
