using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser;

public class EndUserGroup : Group
{
    public const string RoutePrefix = "enduser";
    public EndUserGroup()
    {
        Configure(RoutePrefix, ep =>
        {
            ep.EndpointVersion(1);
        });
    }
}
