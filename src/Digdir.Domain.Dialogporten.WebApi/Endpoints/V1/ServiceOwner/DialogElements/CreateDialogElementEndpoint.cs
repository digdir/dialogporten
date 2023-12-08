using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogElements.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.WebApi.Common;
using FastEndpoints;
using MediatR;
using Medo;
using IMapper = AutoMapper.IMapper;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElements;

public sealed class CreateDialogActivityEndpoint : Endpoint<CreateDialogElementRequest>
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public CreateDialogActivityEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override void Configure()
    {
        Post("dialogs/{dialogId}/elements");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(CreateDialogElementRequest req, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery { DialogId = req.DialogId }, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            await this.NotFoundAsync(entityNotFound, cancellationToken: ct);
            return;
        }

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);

        req.Id = !req.Id.HasValue || req.Id.Value == default
            ? Uuid7.NewUuid7().ToGuid()
            : req.Id;

        updateDialogDto.Elements.Add(req);

        var updateDialogCommand = new UpdateDialogCommand { Id = req.DialogId, ETag = req.ETag, Dto = updateDialogDto };

        var result = await _sender.Send(updateDialogCommand, ct);
        await result.Match(
            success => SendCreatedAtAsync<GetDialogElementEndpoint>(new GetDialogElementQuery { DialogId = dialog.Id, ElementId = req.Id.Value }, req.Id, cancellation: ct),
            notFound => this.NotFoundAsync(notFound, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            concurrencyError => this.PreconditionFailed(cancellationToken: ct));
    }
}

public sealed class CreateDialogElementRequest : UpdateDialogDialogElementDto
{
    public Guid DialogId { get; set; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false)]
    public Guid? ETag { get; set; }
}
