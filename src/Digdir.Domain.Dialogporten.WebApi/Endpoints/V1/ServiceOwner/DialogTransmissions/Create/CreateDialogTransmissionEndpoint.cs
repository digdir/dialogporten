using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogTransmissions.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogTransmissions.Get;
using FastEndpoints;
using MediatR;
using Medo;
using IMapper = AutoMapper.IMapper;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogTransmissions.Create;

public sealed class CreateDialogTransmissionEndpoint : Endpoint<CreateDialogTransmissionRequest>
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public CreateDialogTransmissionEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper;
    }

    public override void Configure()
    {
        Post("dialogs/{dialogId}/transmissions");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => CreateDialogTransmissionSwaggerConfig.SetDescription(b));
    }

    public override async Task HandleAsync(CreateDialogTransmissionRequest req, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery { DialogId = req.DialogId }, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            await this.NotFoundAsync(entityNotFound, cancellationToken: ct);
            return;
        }

        // Remove all existing transmissions, since this list is append only and
        // existing transmissions should not be considered in the new update request.
        dialog.Transmissions.Clear();

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);

        req.Id = !req.Id.HasValue || req.Id.Value == default
            ? Uuid7.NewUuid7().ToGuid()
            : req.Id;

        updateDialogDto.Transmissions.Add(req);

        var updateDialogCommand = new UpdateDialogCommand { Id = req.DialogId, IfMatchDialogRevision = req.IfMatchDialogRevision, Dto = updateDialogDto };

        var result = await _sender.Send(updateDialogCommand, ct);

        await result.Match(
            success => SendCreatedAtAsync<GetDialogTransmissionEndpoint>(new GetDialogTransmissionQuery { DialogId = dialog.Id, TransmissionId = req.Id.Value }, req.Id, cancellation: ct),
            notFound => this.NotFoundAsync(notFound, ct),
            badRequest => this.BadRequestAsync(badRequest, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            forbidden => this.ForbiddenAsync(forbidden, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            concurrencyError => this.PreconditionFailed(cancellationToken: ct));
    }
}

public sealed class CreateDialogTransmissionRequest : UpdateDialogDialogTransmissionDto
{
    public Guid DialogId { get; set; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false, removeFromSchema: true)]
    public Guid? IfMatchDialogRevision { get; set; }
}
