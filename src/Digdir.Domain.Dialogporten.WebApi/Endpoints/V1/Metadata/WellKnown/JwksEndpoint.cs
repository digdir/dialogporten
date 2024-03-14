using Digdir.Domain.Dialogporten.Application.Features.V1.Metadata.WellKnown.Jwks.Queries.Get;
using FastEndpoints;
using MediatR;
using Microsoft.Net.Http.Headers;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Metadata.WellKnown;

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
        Description(x => x.ExcludeFromDescription());
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
}
