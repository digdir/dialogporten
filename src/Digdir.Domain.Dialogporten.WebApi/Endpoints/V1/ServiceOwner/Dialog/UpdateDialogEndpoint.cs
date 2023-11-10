using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
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
        Put("dialogs/{dialogId}");
        Policies(AuthorizationPolicy.Serviceprovider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("ReplaceDialog")
            .ClearDefaultProduces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status412PreconditionFailed)
            .Produces(StatusCodes.Status422UnprocessableEntity)
        );
    }

    public override async Task HandleAsync(UpdateDialogRequest req, CancellationToken ct)
    {
        var command = new UpdateDialogCommand { Id = req.DialogId, ETag = req.ETag, Dto = req.Dto };
        var updateDialogResult = await _sender.Send(command, ct);
        await updateDialogResult.Match(
            success => SendNoContentAsync(ct),
            entityNotFound => this.NotFoundAsync(entityNotFound, ct),
            validationFailed => this.BadRequestAsync(validationFailed, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            concurrencyError => this.PreconditionFailed(ct));
    }
}

public sealed class UpdateDialogRequest
{
    public Guid DialogId { get; set; }

    [FromBody]
    public UpdateDialogDto Dto { get; set; } = null!;

    [FromHeader(headerName: Constants.IfMatch, isRequired: false)]
    public Guid? ETag { get; set; }
}

public sealed class UpdateDialogEndpointSummary : Summary<UpdateDialogEndpoint>
{
    public UpdateDialogEndpointSummary()
    {
        Summary = "Replaces a dialog";
        Description = """
                Replaces a given dialog with the supplied model. For more information see the documentation (link TBD).

                Optimistic concurrency control is implemented using the If-Match header. Supply the ETag value from the GetDialog endpoint to ensure that the dialog is not deleted by another request in the meantime.
                """;
        Responses[StatusCodes.Status204NoContent] = "The dialog was updated successfully";
        Responses[StatusCodes.Status400BadRequest] = Constants.SummaryError400;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SummaryErrorServiceOwner401;
        Responses[StatusCodes.Status403Forbidden] = "Unauthorized to update the supplied dialog (not owned by authenticated organization or has additional scope requirements defined in policy)";
        Responses[StatusCodes.Status404NotFound] = "The given dialog ID was not found or is already deleted";
        Responses[StatusCodes.Status412PreconditionFailed] = "The supplied If-Match header did not match the current ETag value for the dialog. The dialog was not updated.";
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SummaryError422;
    }
}
