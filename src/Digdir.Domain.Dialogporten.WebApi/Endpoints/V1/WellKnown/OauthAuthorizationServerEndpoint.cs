using Digdir.Domain.Dialogporten.Application.Features.V1.WellKnown.OauthAuthorizationServer.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.Net.Http.Headers;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.WellKnown;

public sealed class OauthAuthorizationServerEndpoint : EndpointWithoutRequest<GetOauthAuthorizationServerDto>
{
    private readonly ISender _sender;

    public OauthAuthorizationServerEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get(".well-known/oauth-authorization-server");
        Group<MetadataGroup>();
        Description(b => b
            .OperationId("GetMetadataOauthAuthorizationServer")
            .Produces<GetOauthAuthorizationServerDto>()
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _sender.Send(new GetOauthAuthorizationServerQuery(), ct);

        HttpContext.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = TimeSpan.FromHours(24)
        };
        HttpContext.Response.Headers[HeaderNames.Vary] = new[] { "Accept-Encoding" };

        await SendOkAsync(result, ct);
    }

    public sealed class OauthAuthorizationServerEndpointSummary : Summary<OauthAuthorizationServerEndpoint>
    {
        public OauthAuthorizationServerEndpointSummary()
        {
            Summary = "Gets the OAuth 2.0 Metadata for automatic configuration of clients verifying dialog tokens.";
            Description = """
                          This endpoint can be used by client integrations supporting automatic discovery of "OAuth 2.0 Authorization Server" metadata, enabling verification of dialog tokens issues by Dialogporten.
                          """;
            Responses[StatusCodes.Status200OK] = "The OAuth 2.0 Authorization Server Metadata";
        }
    }
}
