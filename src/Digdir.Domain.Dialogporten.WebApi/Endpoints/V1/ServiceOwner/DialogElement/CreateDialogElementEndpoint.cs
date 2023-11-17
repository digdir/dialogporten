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

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElement;

public sealed class CreateDialogElementEndpoint : Endpoint<CreateDialogElementRequest>
{
    private readonly IMapper _mapper;
    private readonly ISender _sender;

    public CreateDialogElementEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public override void Configure()
    {
        Post("dialogs/{dialogId}/elements");
        Policies(AuthorizationPolicy.Serviceprovider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("CreateDialogElement")
            .ProducesOneOf(
                StatusCodes.Status201Created,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed,
                StatusCodes.Status422UnprocessableEntity)
        );
    }

    public override async Task HandleAsync(CreateDialogElementRequest request, CancellationToken ct)
    {
        var dialogQueryResult = await _sender.Send(new GetDialogQuery {DialogId = request.DialogId}, ct);
        if (dialogQueryResult.TryPickT1(out var entityNotFound, out var dialog))
        {
            await this.NotFoundAsync(entityNotFound, cancellationToken: ct);
            return;
        }

        var updateDialogDto = _mapper.Map<UpdateDialogDto>(dialog);

        request.Id = !request.Id.HasValue || request.Id.Value == default
            ? Uuid7.NewUuid7().ToGuid()
            : request.Id;

        updateDialogDto.Elements.Add(request);

        var updateDialogCommand = new UpdateDialogCommand {Id = request.DialogId, ETag = request.ETag, Dto = updateDialogDto};

        var result = await _sender.Send(updateDialogCommand, ct);
        await result.Match(
            success => SendCreatedAtAsync<GetDialogElementEndpoint>(new GetDialogElementQuery {DialogId = dialog.Id, ElementId = request.Id.Value}, request.Id, cancellation: ct),
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

public sealed class CreateDialogElementEndpointSummary : Summary<CreateDialogElementEndpoint>
{
    public CreateDialogElementEndpointSummary()
    {
        Summary = "Creates a new dialog element";
        Description = $"""
                The dialog element is created with the given configuration. For more information see the documentation (link TBD).

                {Constants.SwaggerSummary.OptimisticConcurrencyNote}
                """;

        ResponseExamples[StatusCodes.Status201Created] = "b6dc8b01-1cd8-2777-b759-d84b0e384f47";

        Responses[StatusCodes.Status201Created] = string.Format(Constants.SwaggerSummary.Created, "element");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure;
        Responses[StatusCodes.Status403Forbidden] = string.Format(Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity, "create");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.EtagMismatch;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
