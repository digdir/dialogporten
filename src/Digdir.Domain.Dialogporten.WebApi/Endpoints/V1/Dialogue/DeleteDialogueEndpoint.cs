using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Delete;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpDelete("dialogue/{id}")]
public sealed class DeleteDialogueEndpoint : Endpoint<DeleteDialogueCommand>
{
    private readonly ISender _sender;

    public DeleteDialogueEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(DeleteDialogueCommand req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct));
    }
}