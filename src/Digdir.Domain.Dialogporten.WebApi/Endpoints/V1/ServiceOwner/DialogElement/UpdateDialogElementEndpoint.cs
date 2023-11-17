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

public sealed class UpdateDialogElementEndpoint : Endpoint<UpdateDialogElementRequest>
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public UpdateDialogElementEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override void Configure()
    {
        Put("dialogs/{dialogId}/elements/{elementId}");
        Policies(AuthorizationPolicy.Serviceprovider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("ReplaceDialogElement")
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed,
                StatusCodes.Status422UnprocessableEntity)
        );
    }

    public override async Task HandleAsync(UpdateDialogElementRequest request, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery {DialogId = request.DialogId}, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            await this.NotFoundAsync(entityNotFound, cancellationToken: ct);
            return;
        }

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);

        var dialogElement = updateDialogDto.Elements.FirstOrDefault(x => x.Id == request.ElementId);
        if (dialogElement is null)
        {
            await this.NotFoundAsync(
                new EntityNotFound<Domain.Dialogs.Entities.DialogElements.DialogElement>(request.ElementId),
                cancellationToken: ct);
            return;
        }

        updateDialogDto.Elements.Remove(dialogElement);

        var updateDialogElementDto = MapToUpdateDto(request);
        updateDialogDto.Elements.Add(updateDialogElementDto);

        var updateDialogCommand = new UpdateDialogCommand
            {Id = request.DialogId, ETag = request.ETag, Dto = updateDialogDto};

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

public sealed class UpdateDialogElementEndpointSummary : Summary<UpdateDialogElementEndpoint>
{
    public UpdateDialogElementEndpointSummary()
    {
        Summary = "Replaces a dialog element";
        Description = """
                Replaces a given dialog element with the supplied model. For more information see the documentation (link TBD).

                Optimistic concurrency control is implemented using the If-Match header. Supply the ETag value from the GetDialog endpoint to ensure that the dialog is not deleted by another request in the meantime.
                """;
        Responses[StatusCodes.Status204NoContent] = string.Format(Constants.SwaggerSummary.Updated, "element");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure;
        Responses[StatusCodes.Status403Forbidden] = string.Format(Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity, "update");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogElementNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.EtagMismatch;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
