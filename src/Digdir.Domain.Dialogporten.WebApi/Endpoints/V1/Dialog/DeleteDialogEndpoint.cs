using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Delete;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialog;

[AllowAnonymous]
[HttpDelete("dialogs/{id}")]
public sealed class DeleteDialogEndpoint : Endpoint<DeleteDialogRequest>
{
    private readonly ISender _sender;

    public DeleteDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(DeleteDialogRequest req, CancellationToken ct)
    {
        var command = new DeleteDialogCommand { Id = req.Id, ETag = req.ETag };
        var result = await _sender.Send(command, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct));
    }
}

public sealed class DeleteDialogRequest
{
    public Guid Id { get; set; }

    [FromHeader(headerName: "x-etag", isRequired: false)]
    public Guid? ETag { get; set; }
}