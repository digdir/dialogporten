using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;
using IMapper = AutoMapper.IMapper;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElements.Delete;

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

        Description(b => DeleteDialogElementSwaggerConfig.SetDescription(b));
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
            await this.NotFoundAsync(new EntityNotFound<DialogElement>(req.ElementId), cancellationToken: ct);
            return;
        }

        updateDialogDto.Elements.Remove(dialogElement);

        var updateDialogCommand = new UpdateDialogCommand
        { Id = req.DialogId, IfMatchDialogRevision = req.IfMatchDialogRevision, Dto = updateDialogDto };

        var result = await _sender.Send(updateDialogCommand, ct);
        await result.Match(
            success => SendNoContentAsync(ct),
            notFound => this.NotFoundAsync(notFound, ct),
            badRequest => this.BadRequestAsync(badRequest, ct),
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
    public Guid? IfMatchDialogRevision { get; set; }
}
