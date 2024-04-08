using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1;

public sealed class MetadataGroup : Group
{
    private const string GroupName = "Metadata";
    public MetadataGroup()
    {
        Configure(string.Empty, ep =>
        {
            ep.DontAutoTag();
            ep.Description(x => x.WithTags(GroupName));
            ep.AllowAnonymous();
            ep.EndpointVersion(1);
        });
    }
}
