using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpPut("dialogues/{id}")]
public sealed class UpdateDialogueEndpoint : Endpoint<UpdateDialogueRequest>
{
    private readonly ISender _sender;

    public UpdateDialogueEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(UpdateDialogueRequest req, CancellationToken ct)
    {
        var command = new UpdateDialogueCommand { Id = req.Id, Dto = req.Dto };
        var updateDialogueResult = await _sender.Send(command, ct);
        await updateDialogueResult.Match(
            success => SendNoContentAsync(ct),
            entityNotFound => this.NotFoundAsync(entityNotFound, ct),
            entityExists => this.ConflictAsync(entityExists, ct),
            validationFailed => this.BadRequestAsync(validationFailed, ct));
    }
}

public class UpdateDialogueRequest
{
    public Guid Id { get; set; }

    [FromBody]
    public UpdateDialogueDto Dto { get; set; } = null!;
}
