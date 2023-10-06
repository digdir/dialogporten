using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner;

public class ServiceOwnerGroup : Group
{
    public const string RoutePrefix = "Serviceowner";
    public ServiceOwnerGroup()
    {
        Configure(RoutePrefix.ToLower(), ep =>
        {
            ep.EndpointVersion(1);
        });
    }
}
