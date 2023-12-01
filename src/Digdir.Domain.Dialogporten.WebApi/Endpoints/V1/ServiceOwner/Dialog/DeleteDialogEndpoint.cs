using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Delete;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
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
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("DeleteDialog")
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed)
        );
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

    [FromHeader(headerName: Constants.IfMatch, isRequired: false, removeFromSchema: true)]
    public Guid? ETag { get; set; }
}

public sealed class DeleteDialogEndpointSummary : Summary<DeleteDialogEndpoint>
{
    public DeleteDialogEndpointSummary()
    {
        Summary = "Deletes a dialog";
        Description = """
                Deletes a given dialog (soft delete). For more information see the documentation (link TBD).

                Note that the dialog will still be available on the single details endpoint, but will have a deleted status. It will not appear on the list endpoint for either service owners nor end users.
                If end users attempt to access the dialog via the details endpoint, they will get a 410 Gone response.

                Optimistic concurrency control is implemented using the If-Match header. Supply the ETag value from the GetDialog endpoint to ensure that the dialog is not deleted by another request in the meantime.
                """;
        Responses[StatusCodes.Status204NoContent] = string.Format(Constants.SwaggerSummary.Deleted, "aggregate");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure;
        Responses[StatusCodes.Status403Forbidden] = string.Format(Constants.SwaggerSummary.AccessDeniedToDialog, "delete");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.EtagMismatch;
    }
}
