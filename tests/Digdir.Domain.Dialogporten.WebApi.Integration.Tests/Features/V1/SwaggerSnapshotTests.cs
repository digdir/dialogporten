using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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
        // The order of the properties in the swagger.json file is not cross-platform deterministic.
        // Running client.GetAsync("/swagger/v1/swagger.json"); on Windows and Mac will produce
        // different ordering of the results (although the content is the same). So we force an
        // alphabetical ordering of the properties to make the test deterministic.  
        // Ref: https://github.com/digdir/dialogporten/issues/996
        var orderedSwagger = SortJson(newSwagger);

        // Assert
        response.EnsureSuccessStatusCode();

        await Verify(orderedSwagger, extension: "json")
            .UseFileName("swagger")
            .UseDirectory(swaggerPath);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    private static string SortJson(string jsonString)
    {
        using var document = JsonDocument.Parse(jsonString);
        var sortedElement = SortElement(document.RootElement);
        return JsonSerializer.Serialize(sortedElement, SerializerOptions);
    }

    [SuppressMessage("Style", "IDE0010:Add missing cases")]
    private static JsonElement SortElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var sortedProperties = new SortedDictionary<string, JsonElement>();
                    foreach (var property in element.EnumerateObject())
                    {
                        sortedProperties[property.Name] = SortElement(property.Value);
                    }

                    var jsonDocument = JsonDocument.Parse(JsonSerializer.Serialize(sortedProperties));
                    return jsonDocument.RootElement;
                }
            case JsonValueKind.Array:
                {
                    var sortedArray = element
                        .EnumerateArray()
                        .Select(SortElement)
                        .ToList();
                    var arrayDocument = JsonDocument.Parse(JsonSerializer.Serialize(sortedArray));
                    return arrayDocument.RootElement;
                }
            default:
                return element;
        }
    }
}
