using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Dialog;

[AllowAnonymous]
[HttpPut("dialogs/{id}")]
public sealed class UpdateDialogEndpoint : Endpoint<UpdateDialogRequest>
{
    private readonly ISender _sender;

    public UpdateDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override async Task HandleAsync(UpdateDialogRequest req, CancellationToken ct)
    {
        var command = new UpdateDialogCommand { Id = req.Id, ETag = req.ETag, Dto = req.Dto };
        var updateDialogResult = await _sender.Send(command, ct);
        await updateDialogResult.Match(
            success => SendNoContentAsync(ct),
            entityNotFound => this.NotFoundAsync(entityNotFound, ct),
            entityExists => this.ConflictAsync(entityExists, ct),
            validationFailed => this.BadRequestAsync(validationFailed, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct));
    }
}

public sealed class UpdateDialogRequest
{
    public Guid Id { get; set; }

    [FromBody]
    public UpdateDialogDto Dto { get; set; } = null!;

    [FromHeader(headerName: "x-etag", isRequired: false)]
    public Guid? ETag { get; set; }
}