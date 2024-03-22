using Digdir.Domain.Dialogporten.Application.Features.V1.WellKnown.OauthAuthorizationServer.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.WellKnown.OauthAuthorizationServer.Get;

public class GetOauthAuthorizationServerSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetOauthAuthorizationServer";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId)
            .ProducesOneOf<GetOauthAuthorizationServerDto>(
                StatusCodes.Status200OK);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class GetOauthAuthorizationServerEndpointSummary : Summary<GetOauthAuthorizationServerEndpoint>
{
    public GetOauthAuthorizationServerEndpointSummary()
    {
        Summary = "Gets the OAuth 2.0 Metadata for automatic configuration of clients verifying dialog tokens.";
        Description = """
                      This endpoint can be used by client integrations supporting automatic discovery of "OAuth 2.0 Authorization Server" metadata, enabling verification of dialog tokens issues by Dialogporten.
                      """;
        Responses[StatusCodes.Status200OK] = "The OAuth 2.0 Authorization Server Metadata";
    }
}
