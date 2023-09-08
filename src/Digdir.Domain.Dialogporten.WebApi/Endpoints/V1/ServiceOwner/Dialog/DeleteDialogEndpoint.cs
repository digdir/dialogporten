﻿using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Delete;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

public sealed class DeleteDialogEndpoint : Endpoint<DeleteDialogRequest>
{
    private readonly ISender _sender;

    public DeleteDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Delete("dialogs/{dialogId}");
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(DeleteDialogRequest req, CancellationToken ct)
    {
        var command = new DeleteDialogCommand { Id = req.DialogId, ETag = req.ETag };
        var result = await _sender.Send(command, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            concurrencyError => this.PreconditionFailed(ct));
    }
}

public sealed class DeleteDialogRequest
{
    public Guid DialogId { get; set; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false)]
    public Guid? ETag { get; set; }
}