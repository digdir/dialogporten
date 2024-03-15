using Digdir.Domain.Dialogporten.Application.Features.V1.WellKnown.Jwks.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;
using Microsoft.Net.Http.Headers;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.WellKnown;

public sealed class JwksEndpoint : EndpointWithoutRequest<GetJwksDto>
{
    private readonly ISender _sender;

    public JwksEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get(".well-known/jwks.json");
        Group<MetadataGroup>();
        Description(b => b
            .OperationId("GetMetadataJwks")
            .Produces<GetJwksDto>()
        );
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _sender.Send(new GetJwksQuery(), ct);

        HttpContext.Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue()
        {
            Public = true,
            MaxAge = TimeSpan.FromHours(24)
        };
        HttpContext.Response.Headers[HeaderNames.Vary] = new[] { "Accept-Encoding" };


        await SendOkAsync(result, ct);
    }

    public sealed class JwksEndpointSummary : Summary<JwksEndpoint>
    {
        public JwksEndpointSummary()
        {
            Summary = "Gets the JSON Web Key Set (JWKS) containing the public keys used to verify dialog token signatures";
            Description = """
                          This endpoint can be used by client integrations supporting automatic discovery of "OAuth 2.0 Authorization Server" metadata, enabling verification of dialog tokens issues by Dialogporten.
                          """;
            Responses[StatusCodes.Status200OK] = "The OAuth 2.0 Authorization Server Metadata";
        }
    }
}
