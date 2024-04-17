using Digdir.Domain.Dialogporten.GraphQL;
using Digdir.Domain.Dialogporten.GraphQL.EndUser;
using HotChocolate.Execution;
using Microsoft.AspNetCore.Builder;
using Path = System.IO.Path;
using Microsoft.Extensions.DependencyInjection;

namespace Digdir.Domain.Dialogporten.GraphQl.Integration.Tests.Schema;

public class SchemaSnapshotTests
{
    // private readonly WebApplicationFactory<Program> _webApplicationFactory;

    public SchemaSnapshotTests()
    {
        // _webApplicationFactory = webApplicationFactory;
    }

    [Fact]
    public async Task FailIfGraphQlSchemaSnapshotDoesNotMatch()
    {
        // Arrange
        // This test checks for changes against the published version of the schema.verified.graphql file
        // The file is located at /docs/schema/ on the solution root
        // Commiting a change to this file will trigger a build and publish
        // of the npm package located in the same folder
        var rootPath = Utils.GetSolutionRootFolder();
        var schemaPath = Path.Combine(rootPath!, "docs/schema/V1");

        var builder = WebApplication.CreateBuilder([]);
        builder.Services.AddDialogportenGraphQl();

        var app = builder.Build();
        var requestExecutor =
            await app.Services.GetRequiredService<IRequestExecutorResolver>().GetRequestExecutorAsync();

        // Act
        var schema = requestExecutor.Schema.Print();

        // Assert
        await Verify(schema, extension: "graphql")
            .UseFileName("schema")
            .UseDirectory(schemaPath);
    }
}
