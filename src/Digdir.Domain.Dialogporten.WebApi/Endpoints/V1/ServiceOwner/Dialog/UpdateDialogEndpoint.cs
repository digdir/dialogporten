using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.WebApi.Common;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

public sealed class UpdateDialogEndpoint : Endpoint<UpdateDialogRequest>
{
    private readonly ISender _sender;

    public UpdateDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Put("dialogs/{id}");
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(UpdateDialogRequest req, CancellationToken ct)
    {
        var command = new UpdateDialogCommand { Id = req.Id, ETag = req.ETag, Dto = req.Dto };
        var updateDialogResult = await _sender.Send(command, ct);
        await updateDialogResult.Match(
            success => SendNoContentAsync(ct),
            entityNotFound => this.NotFoundAsync(entityNotFound, ct),
            validationFailed => this.BadRequestAsync(validationFailed, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct));
    }
}

public sealed class UpdateDialogRequest
{
    public Guid Id { get; set; }

    [FromBody]
    public UpdateDialogDto Dto { get; set; } = null!;

    [FromHeader(headerName: Constants.IfMatch, isRequired: false)]
    public Guid? ETag { get; set; }
}