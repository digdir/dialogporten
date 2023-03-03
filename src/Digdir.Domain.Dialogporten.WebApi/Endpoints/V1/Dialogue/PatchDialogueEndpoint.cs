using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;
using FastEndpoints;
using Json.Patch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialogue;

[AllowAnonymous]
[HttpPatch("dialogue/{id}")]
public sealed class PatchDialogueEndpoint : Endpoint<PatchDialogueRequest>
{
    private readonly ISender _sender;

    public PatchDialogueEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(PatchDialogueRequest req, CancellationToken ct)
    {
        var command = new UpdateDialogueCommand { Id = req.Id, Dto = req.PatchDocument };
        var result = await _sender.Send(command, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            entityNotFound => this.NotFoundAsync(entityNotFound, ct),
            entityExists => this.ConflictAsync(entityExists, ct),
            validationFailed => this.BadRequestAsync(validationFailed, ct));
    }
}

public class PatchDialogueRequest : IPlainTextRequest
{
    public Guid Id { get; set; }

    [FromBody]
    public string Content { get; set; } = null!;
    public JsonPatch PatchDocument => JsonSerializer.Deserialize<JsonPatch>(Content);
}