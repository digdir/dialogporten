using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Create;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpPost("dialogue")]
public sealed class CreateDialogueEndpoint : Endpoint<CreateDialogueCommand>
{
    private readonly ISender _sender;

    public CreateDialogueEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(CreateDialogueCommand req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            id => SendCreatedAtAsync<GetDialogueEndpoint>(new { id }, id, cancellation: ct),
            entityExists => this.ConflictAsync(entityExists, ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}
