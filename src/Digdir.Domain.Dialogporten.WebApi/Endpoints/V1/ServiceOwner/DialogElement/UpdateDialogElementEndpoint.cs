using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Update;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;
using IMapper = AutoMapper.IMapper;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElement;

public sealed class UpdateDialogActivityEndpoint : Endpoint<UpdateDialogElementRequest>
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public UpdateDialogActivityEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override void Configure()
    {
        Put("dialogs/{dialogId}/elements/{elementId}");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(UpdateDialogElementRequest req, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery { DialogId = req.DialogId }, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            await this.NotFoundAsync(entityNotFound, cancellationToken: ct);
            return;
        }

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);

        var dialogElement = updateDialogDto.Elements.FirstOrDefault(x => x.Id == req.ElementId);
        if (dialogElement is null)
        {
            await this.NotFoundAsync(
                new EntityNotFound<Domain.Dialogs.Entities.Elements.DialogElement>(req.ElementId),
                cancellationToken: ct);
            return;
        }

        updateDialogDto.Elements.Remove(dialogElement);

        var updateDialogElementDto = MapToUpdateDto(req);
        updateDialogDto.Elements.Add(updateDialogElementDto);

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

    private static UpdateDialogDialogElementDto MapToUpdateDto(UpdateDialogElementRequest request) => new()
    {
        Id = request.ElementId,
        Type = request.Type,
        AuthorizationAttribute = request.AuthorizationAttribute,
        RelatedDialogElementId = request.RelatedDialogElementId,
        DisplayName = request.DisplayName,
        Urls = request.Urls
    };
}

public sealed class UpdateDialogElementRequest
{
    public Guid DialogId { get; set; }

    public Guid ElementId { get; set; }

    [FromHeader(headerName: Constants.IfMatch, isRequired: false)]
    public Guid? ETag { get; set; }

    public Uri? Type { get; set; }
    public string? AuthorizationAttribute { get; set; }

    public Guid? RelatedDialogElementId { get; set; }

    public List<LocalizationDto> DisplayName { get; set; } = new();
    public List<UpdateDialogDialogElementUrlDto> Urls { get; set; } = new();
}
