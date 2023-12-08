using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

public sealed class CreateDialogEndpoint : Endpoint<CreateDialogCommand>
{
    private readonly ISender _sender;

    public CreateDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Post("dialogs");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("CreateDialog")
            .ProducesOneOf(
                StatusCodes.Status201Created,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status422UnprocessableEntity)
        );
    }

    public override async Task HandleAsync(CreateDialogCommand req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            success => SendCreatedAtAsync<GetDialogEndpoint>(new GetDialogQuery { DialogId = success.Value }, success.Value, cancellation: ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            unauthorized => SendUnauthorizedAsync(ct));
    }
}

public sealed class CreateDialogEndpointSummary : Summary<CreateDialogEndpoint>
{
    public CreateDialogEndpointSummary()
    {
        Summary = "Creates a new dialog";
        Description = """
                The dialog is created with the given configuration. For more information see the documentation (link TBD).

                For detailed information on validation rules, see [the source for CreateDialogCommandValidator](https://github.com/digdir/dialogporten/blob/main/src/Digdir.Domain.Dialogporten.Application/Features/V1/ServiceOwner/Dialogs/Commands/Create/CreateDialogCommandValidator.cs)
                """;

        ResponseExamples[StatusCodes.Status201Created] = "018bb8e5-d9d0-7434-8ec5-569a6c8e01fc";

        Responses[StatusCodes.Status201Created] = Constants.SwaggerSummary.Created.FormatInvariant("aggregate");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.DialogCreationNotAllowed;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
