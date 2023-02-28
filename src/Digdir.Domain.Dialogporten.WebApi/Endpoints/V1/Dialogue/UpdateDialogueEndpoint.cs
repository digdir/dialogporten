using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpPut("dialogue/{id}")]
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
        var updateDualogueResult = await _sender.Send(command, ct);
        await updateDualogueResult.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            validationFailed => this.BadRequestAsync(validationFailed, ct));
    }
}

public class UpdateDialogueRequest
{
    public Guid Id { get; set; }

    [FromBody]
    public UpdateDialogueDto Dto { get; set; } = null!;
}
