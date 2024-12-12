using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Update;
using NJsonSchema;
using NSwag;

namespace Digdir.Domain.Dialogporten.WebApi;

public static class OpenApiDocumentExtensions
{
    /// <summary>
    /// To have this be validated in BlackDuck, we need to lower case the bearer scheme name.
    /// From editor.swagger.io:
    /// Structural error at components.securitySchemes.JWTBearerAuth
    /// should NOT have a `bearerFormat` property without `scheme: bearer` being set
    /// </summary>
    /// <param name="openApiDocument"></param>
    public static void FixJwtBearerCasing(this OpenApiDocument openApiDocument)
    {
        foreach (var securityScheme in openApiDocument.Components.SecuritySchemes.Values)
        {
            if (securityScheme.Scheme.Equals("Bearer", StringComparison.Ordinal))
            {
                securityScheme.Scheme = "bearer";
            }
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

    public static void MakeCollectionsNullable(this OpenApiDocument openApiDocument)
    {
        foreach (var schema in openApiDocument.Components.Schemas.Values)
        {
            MakeCollectionsNullable(schema);
        }
    }

    private static void MakeCollectionsNullable(JsonSchema schema)
    {
        if (schema.Properties == null)
        {
            return;
        }

        foreach (var property in schema.Properties.Values)
        {
            if (property.Type.HasFlag(JsonObjectType.Array))
            {
                property.IsNullableRaw = true;
            }
        }
    }
}
