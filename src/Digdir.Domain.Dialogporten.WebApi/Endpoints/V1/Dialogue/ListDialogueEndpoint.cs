using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.List;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpGet("dialogue")]
public class ListDialogueEndpoint : Endpoint<ListDialogueQuery>
{
    private readonly ISender _sender;

    public ListDialogueEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(ListDialogueQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            paginatedDto => SendOkAsync(paginatedDto, ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}