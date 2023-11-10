namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder OperationId(this RouteHandlerBuilder builder, string operationId)
    {
        builder.Add(b => b.Metadata.Add(new EndpointNameMetadata(operationId)));
        return builder;
    }
}
