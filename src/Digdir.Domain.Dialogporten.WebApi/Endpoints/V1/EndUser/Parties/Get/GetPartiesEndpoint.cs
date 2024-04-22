using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.Parties.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.Parties.Get;

public class GetPartiesEndpoint : EndpointWithoutRequest<GetPartiesDto>
{
    private readonly ISender _sender;

    public GetPartiesEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("parties");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(d => GetPartiesSwaggerConfig.SetDescription(d));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _sender.Send(new GetPartiesQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
