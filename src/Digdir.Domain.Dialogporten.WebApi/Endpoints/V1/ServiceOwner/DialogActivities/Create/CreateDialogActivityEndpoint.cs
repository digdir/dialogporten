using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogActivities.Get;
using FastEndpoints;
using MediatR;
using Medo;
using IMapper = AutoMapper.IMapper;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogActivities.Create;

public sealed class CreateDialogActivityEndpoint : Endpoint<CreateDialogActivityRequest>
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public CreateDialogActivityEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper;
    }

    public override void Configure()
    {
        Post("dialogs/{dialogId}/activities");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => CreateDialogActivitySwaggerConfig.SetDescription(b));
    }

    public override async Task HandleAsync(CreateDialogActivityRequest req, CancellationToken ct)
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

        req.Id = !req.Id.HasValue || req.Id.Value == default
            ? Uuid7.NewUuid7().ToGuid()
            : req.Id;

        updateDialogDto.Activities.Add(req);

        var updateDialogCommand = new UpdateDialogCommand { Id = req.DialogId, IfMatchDialogRevision = req.IfMatchDialogRevision, Dto = updateDialogDto };

        var result = await _sender.Send(updateDialogCommand, ct);
        await result.Match(
            success => SendCreatedAtAsync<GetDialogActivityEndpoint>(new GetDialogActivityQuery { DialogId = dialog.Id, ActivityId = req.Id.Value }, req.Id, cancellation: ct),
            notFound => this.NotFoundAsync(notFound, ct),
            badRequest => this.BadRequestAsync(badRequest, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            concurrencyError => this.PreconditionFailed(cancellationToken: ct));
    }
}

public sealed class CreateDialogActivityRequest : UpdateDialogDialogActivityDto
{
    public Guid DialogId { get; set; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false, removeFromSchema: true)]
    public Guid? IfMatchDialogRevision { get; set; }
}
