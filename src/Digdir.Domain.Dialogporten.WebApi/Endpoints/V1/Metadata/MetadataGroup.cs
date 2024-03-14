using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Metadata;

public sealed class MetadataGroup : Group
{
    public const string RoutePrefix = "Metadata";
    public MetadataGroup()
    {
        Configure(RoutePrefix.ToLowerInvariant(), ep =>
        {
            ep.AllowAnonymous();
            ep.EndpointVersion(1);
        });
    }
}
