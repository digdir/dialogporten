using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs;

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
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("ReplaceDialog")
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed,
                StatusCodes.Status422UnprocessableEntity)
        );
    }

    public override async Task HandleAsync(UpdateDialogRequest req, CancellationToken ct)
    {
        var command = new UpdateDialogCommand { Id = req.DialogId, IfMatchDialogRevision = req.IfMatchDialogRevision, Dto = req.Dto };
        var updateDialogResult = await _sender.Send(command, ct);
        await updateDialogResult.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            badRequest => this.BadRequestAsync(badRequest, ct),
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

    [FromHeader(headerName: Constants.IfMatch, isRequired: false, removeFromSchema: true)]
    public Guid? IfMatchDialogRevision { get; set; }
}

public sealed class UpdateDialogEndpointSummary : Summary<UpdateDialogEndpoint>
{
    public UpdateDialogEndpointSummary()
    {
        Summary = "Replaces a dialog";
        Description = $"""
                Replaces a given dialog with the supplied model. For more information see the documentation (link TBD).

                {Constants.SwaggerSummary.OptimisticConcurrencyNote}
                """;
        Responses[StatusCodes.Status204NoContent] = Constants.SwaggerSummary.Updated.FormatInvariant("aggregate");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("update");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
