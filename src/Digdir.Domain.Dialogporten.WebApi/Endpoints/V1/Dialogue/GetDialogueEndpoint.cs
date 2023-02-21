using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpGet("dialogue/{id}")]
public class GetDialogueEndpoint : Endpoint<GetDialogueQuery>
{
    private readonly ISender _sender;

    public GetDialogueEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(GetDialogueQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await SendOkAsync(result, ct);
    }
}
