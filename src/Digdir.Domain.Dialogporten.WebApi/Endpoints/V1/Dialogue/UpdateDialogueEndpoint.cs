using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpPut("dialogue/{id}")]
public sealed class UpdateDialogueEndpoint : Endpoint<UpdateDialogueCommand>
{
    private readonly ISender _sender;

    public UpdateDialogueEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(UpdateDialogueCommand req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await SendNoContentAsync(ct);
    }
}
