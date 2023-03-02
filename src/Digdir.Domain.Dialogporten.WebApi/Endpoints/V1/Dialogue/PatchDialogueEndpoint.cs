using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogues.Queries.Get;
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
        var getDialogueResult = await _sender.Send(new GetDialogueQuery { Id = req.Id }, ct);
        if (!getDialogueResult.TryPickT0(out var dialogue, out var _))
        {
            await SendNotFoundAsync(ct);
        }

        var updateDialogueDto = req.PatchDocument!.Apply<GetDialogueDto, UpdateDialogueDto>(dialogue);
        var updateDialogueResult = await _sender.Send(new UpdateDialogueCommand { Id = req.Id, Dto = updateDialogueDto! }, ct);
        await updateDialogueResult.Match(
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
    public JsonPatch? PatchDocument => JsonSerializer.Deserialize<JsonPatch>(Content);
}