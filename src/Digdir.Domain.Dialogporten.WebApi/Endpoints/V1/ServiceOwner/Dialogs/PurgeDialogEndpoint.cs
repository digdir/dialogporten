using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Purge;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs;

public sealed class PurgeDialogEndpoint : Endpoint<PurgeDialogRequest>
{
    private readonly ISender _sender;

    public PurgeDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Post("dialogs/{dialogId}/actions/purge");
        RequestBinder(new PurgeDialogRequestBinder());
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("PurgeDialog")
            .Accepts<PurgeDialogRequest>()
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed)
        );
    }

    public override async Task HandleAsync(PurgeDialogRequest req, CancellationToken ct)
    {
        var command = new PurgeDialogCommand { DialogId = req.DialogId, IfMatchDialogRevision = req.IfMatchDialogRevision };
        var result = await _sender.Send(command, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            concurrencyError => this.PreconditionFailed(ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}

public sealed class PurgeDialogRequest
{
    public Guid DialogId { get; init; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false, removeFromSchema: true)]
    public Guid? IfMatchDialogRevision { get; init; }
}

public sealed class PurgeDialogEndpointSummary : Summary<PurgeDialogEndpoint>
{
    public PurgeDialogEndpointSummary()
    {
        Summary = "Permanently deletes a dialog";
        Description = """
                Deletes a given dialog (hard delete). For more information see the documentation (link TBD).

                Optimistic concurrency control is implemented using the If-Match header. Supply the Revision value from the GetDialog endpoint to ensure that the dialog is not deleted by another request in the meantime.
                """;
        Responses[StatusCodes.Status204NoContent] = Constants.SwaggerSummary.Deleted.FormatInvariant("aggregate");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("delete");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
    }
}

// Custom request binder to avoid attempted automatic deserialization of the Request body if the content type is application/json
public class PurgeDialogRequestBinder : IRequestBinder<PurgeDialogRequest>
{
    public ValueTask<PurgeDialogRequest> BindAsync(BinderContext ctx, CancellationToken ct)
    {
        if (!Guid.TryParse(ctx.HttpContext.Request.RouteValues["dialogId"]?.ToString()!, out var dialogId))
            return ValueTask.FromResult(new PurgeDialogRequest());

        ctx.HttpContext.Request.Headers.TryGetValue(Constants.IfMatch, out var revisionHeader);
        var revisionFound = Guid.TryParse(revisionHeader, out var revision);

        return ValueTask.FromResult(new PurgeDialogRequest
        {
            DialogId = dialogId,
            IfMatchDialogRevision = revisionFound ? revision : null
        });

    }
}
