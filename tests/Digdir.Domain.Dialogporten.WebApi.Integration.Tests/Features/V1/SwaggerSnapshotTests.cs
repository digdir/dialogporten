using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Digdir.Domain.Dialogporten.WebApi.Integration.Tests.Features.V1;

public class SwaggerSnapshotTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public SwaggerSnapshotTests(WebApplicationFactory<Program> webApplicationFactory)
    {
        _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task FailIfSwaggerSnapshotDoesNotMatch()
    {
        // Arrange
        // This test checks for changes against the published version of the swagger.verified.json file
        // The file is located at /docs/schema/ on the solution root
        // Commiting a change to this file will trigger a build and publish
        // of the npm package located in the same folder
        var rootPath = Utils.GetSolutionRootFolder();
        var swaggerPath = Path.Combine(rootPath!, "docs/schema/V1");

        var client = _webApplicationFactory
            .WithWebHostBuilder(builder => builder.UseEnvironment("test"))
            .CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var newSwagger = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();

        await Verify(newSwagger, extension: "json")
            .UseFileName("swagger")
            .UseDirectory(swaggerPath);
    }
}
