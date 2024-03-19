using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Update;
using NJsonSchema;
using NSwag;

namespace Digdir.Domain.Dialogporten.WebApi;

public static class OpenApiDocumentExtensions
{
    /// <summary>
    /// This is a workaround for a bug/weird behaviour in FastEndpoints where examples
    /// to the RequestExamples array are not added to the OpenAPI document.
    /// Each SwaggerConfig file implements a GetExample method that returns the example
    ///
    /// This sets for those who have a request body.
    /// </summary>
    /// <param name="openApiDocument"></param>
    public static void ReplaceRequestExampleBodies(this OpenApiDocument openApiDocument)
    {
        foreach (var path in openApiDocument.Paths)
        {
            foreach (var openApiOperation in path.Value.Values)
            {
                openApiOperation.ReplaceRequestExampleBody();
            }
        }
    }

    private static void ReplaceRequestExampleBody(this OpenApiOperation openApiOperation)
    {
        if (openApiOperation.RequestBody?.Content == null)
        {
            return;
        }

        var operationId = openApiOperation.OperationId;

        // TEMP hard coding of operationId, there is only one endpoint with a request body example
        // More to follow, make look up function based on operationId
        if (operationId != "ReplaceDialog") return;

        foreach (var (_, value) in openApiOperation.RequestBody.Content)
        {
            var example = UpdateDialogSwaggerConfig.GetExample();
            value.Example = example;
        }
    }

    /// <summary>
    /// When generating ProblemDetails and ProblemDetails_Error, there is a bug/weird behavior in NSwag or FastEndpoints
    /// which results in certain 'Description' properties being generated when running on f.ex. MacOS,
    /// but not when running on the Ubuntu GitHub Actions runner. This leads to the OpenAPI swagger snapshot test
    /// behaving differently on different platforms/CPU architectures, which is not desirable.
    ///
    /// This method removes these descriptions.
    /// </summary>
    /// <param name="openApiDocument"></param>
    public static void ReplaceProblemDetailsDescriptions(this OpenApiDocument openApiDocument)
    {
        var schemas = openApiDocument.Components.Schemas;
        List<JsonSchema> schemaList = [schemas["ProblemDetails"], schemas["ProblemDetails_Error"]];

        foreach (var schema in schemaList)
        {
            if (schema.Description != null)
            {
                schema.Description = null;
            }

            if (schema.Properties == null)
            {
                continue;
            }

            foreach (var property in schema.Properties)
            {
                if (property.Value.Description != null)
                {
                    property.Value.Description = null;
                }
            }
        }
    }
}
