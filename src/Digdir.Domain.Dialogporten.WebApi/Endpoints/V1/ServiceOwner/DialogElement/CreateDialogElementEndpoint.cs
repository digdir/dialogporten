using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.WebApi.Common;
using FastEndpoints;
using MediatR;
using Medo;
using IMapper = AutoMapper.IMapper;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElement;

public sealed class CreateDialogActivityEndpoint : Endpoint<CreateDialogElementRequest>
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
        Post("dialogs/{dialogId}/elements");
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(CreateDialogElementRequest request, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery {DialogId = request.DialogId}, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            await this.NotFoundAsync(entityNotFound, cancellationToken: ct);
            return;
        }

        // Remove all existing activities, since this list is append only and
        // existing activities should not be considered in the new update request.
        dialog.Activities.Clear();

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);

        request.Id = !request.Id.HasValue || request.Id.Value == default
            ? Uuid7.NewUuid7().ToGuid()
            : request.Id;

        updateDialogDto.Elements.Add(request);

        var updateDialogCommand = new UpdateDialogCommand
            {Id = request.DialogId, ETag = request.ETag, Dto = updateDialogDto};

        var result = await _sender.Send(updateDialogCommand, ct);
        await result.Match(
            success => SendCreatedAtAsync<GetDialogElementEndpoint>(new {request.Id}, request.Id, cancellation: ct),
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