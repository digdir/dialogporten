using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.Parties.Get;

public class GetPartiesSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetParties";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId)
            .Produces<List<GetPartiesDto>>();

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class GetPartiesEndpointSummary : Summary<GetPartiesEndpoint>
{
    public GetPartiesEndpointSummary()
    {
        Summary = "Gets the list of authorized parties for the end user";
        Description = """
                      Gets the list of authorized parties for the end user. For more information see the documentation (link TBD).
                      """;

        Responses[StatusCodes.Status200OK] = "The list of authorized parties for the end user";
    }
}
