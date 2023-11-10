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
        Policies(AuthorizationPolicy.Serviceprovider);
        Group<ServiceOwnerGroup>();

        Description(b => b
            .OperationId("CreateDialog")
            .ProducesProblemDetails()
            .ProducesProblemDetails(statusCode: StatusCodes.Status422UnprocessableEntity)
            .ClearDefaultProduces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status201Created, "application/json")
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

        Responses[StatusCodes.Status201Created] = "The UUID of the created dialog. A relative URL to the newly created dialog is set in the \"Location\" header";
        Responses[StatusCodes.Status400BadRequest] = Constants.SummaryError400;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SummaryErrorServiceOwner401;
        Responses[StatusCodes.Status403Forbidden] = "Unauthorized to create a dialog for the given serviceResource (not owned by authenticated organization or has additional scope requirements defined in policy)";
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SummaryError422;
    }
}
