using Digdir.Domain.Dialogporten.Application.Features.V1.WellKnown.Jwks.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.WellKnown.Jwks.Get;

public class GetJwksSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetJwks";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId)
            .ProducesOneOf<GetJwksDto>(
                StatusCodes.Status200OK);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class GetJwksEndpointSummary : Summary<GetJwksEndpoint>
{
    public GetJwksEndpointSummary()
    {
        Summary = "Gets the JSON Web Key Set (JWKS) containing the public keys used to verify dialog token signatures";
        Description = """
                      This endpoint can be used by client integrations supporting automatic discovery of "OAuth 2.0 Authorization Server" metadata, enabling verification of dialog tokens issues by Dialogporten.
                      """;
        Responses[StatusCodes.Status200OK] = "The OAuth 2.0 Authorization Server Metadata";
    }
}
