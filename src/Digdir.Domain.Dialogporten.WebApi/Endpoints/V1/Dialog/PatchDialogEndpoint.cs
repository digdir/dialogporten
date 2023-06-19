using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;
using FastEndpoints;
using Json.Patch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialog;

[AllowAnonymous]
[HttpPatch("dialogs/{id}")]
public sealed class PatchDialogEndpoint : Endpoint<PatchDialogRequest>
{
    private readonly ISender _sender;

    public PatchDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(PatchDialogRequest req, CancellationToken ct)
    {
        var command = new UpdateDialogCommand { Id = req.Id, Dto = req.PatchDocument };
        var result = await _sender.Send(command, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            entityNotFound => this.NotFoundAsync(entityNotFound, ct),
            entityExists => this.ConflictAsync(entityExists, ct),
            validationFailed => this.BadRequestAsync(validationFailed, ct));
    }
}

public class PatchDialogRequest : IPlainTextRequest
{
    public Guid Id { get; set; }

    [FromBody]
    public string Content { get; set; } = null!;
    public JsonPatch PatchDocument => JsonSerializer.Deserialize<JsonPatch>(Content);
}
