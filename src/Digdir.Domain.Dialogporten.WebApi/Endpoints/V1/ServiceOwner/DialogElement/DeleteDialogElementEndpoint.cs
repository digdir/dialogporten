using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;
using IMapper = AutoMapper.IMapper;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElement;

public sealed class DeleteDialogElementEndpoint : Endpoint<DeleteDialogElementRequest>
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public DeleteDialogElementEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override void Configure()
    {
        Delete("dialogs/{dialogId}/elements/{elementId}");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("DeleteDialogElement")
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed,
                StatusCodes.Status422UnprocessableEntity)
        );
    }

    public override async Task HandleAsync(DeleteDialogElementRequest req, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery { DialogId = req.DialogId }, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            await this.NotFoundAsync(entityNotFound, cancellationToken: ct);
            return;
        }

        // Remove all existing activities, since this list is append only and
        // existing activities should not be considered in the new update request.
        dialog.Activities.Clear();

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);

        var dialogElement = updateDialogDto.Elements.FirstOrDefault(x => x.Id == req.ElementId);
        if (dialogElement is null)
        {
            await this.NotFoundAsync(new EntityNotFound<Domain.Dialogs.Entities.DialogElements.DialogElement>(req.ElementId), cancellationToken: ct);
            return;
        }

        updateDialogDto.Elements.Remove(dialogElement);

        var updateDialogCommand = new UpdateDialogCommand
        { Id = req.DialogId, ETag = req.ETag, Dto = updateDialogDto };

        var result = await _sender.Send(updateDialogCommand, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            concurrencyError => this.PreconditionFailed(cancellationToken: ct));
    }
}

public sealed class DeleteDialogElementRequest
{
    public Guid DialogId { get; set; }
    public Guid ElementId { get; set; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false, removeFromSchema: true)]
    public Guid? ETag { get; set; }
}

public sealed class DeleteDialogElementEndpointSummary : Summary<DeleteDialogElementEndpoint>
{
    public DeleteDialogElementEndpointSummary()
    {
        Summary = "Deletes a dialog element";
        Description = $"""
                Deletes a given dialog element (hard delete). For more information see the documentation (link TBD).

                {Constants.SwaggerSummary.OptimisticConcurrencyNote}
                """;
        Responses[StatusCodes.Status204NoContent] = Constants.SwaggerSummary.Deleted.FormatInvariant("element");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("delete");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogElementNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.EtagMismatch;
    }
}

