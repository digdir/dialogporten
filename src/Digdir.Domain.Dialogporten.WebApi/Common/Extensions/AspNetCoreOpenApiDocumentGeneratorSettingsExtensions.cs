using Digdir.Domain.Dialogporten.WebApi.Common.Json;
using NSwag;
using NSwag.Generation.AspNetCore;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal static class AspNetCoreOpenApiDocumentGeneratorSettingsExtensions
{
    public static AspNetCoreOpenApiDocumentGeneratorSettings CleanupPaginatedLists(
        this AspNetCoreOpenApiDocumentGeneratorSettings settings)
    {
        settings.OperationProcessors.Add(new PaginatedListParametersProcessor());
        return settings;
    }

    public static AspNetCoreOpenApiDocumentGeneratorSettings AddServiceOwnerSuffixToSchemas(
        this AspNetCoreOpenApiDocumentGeneratorSettings settings)
    {
        settings.SchemaNameGenerator = new SuffixedSchemaNameGenerator();
        return settings;
    }
}
