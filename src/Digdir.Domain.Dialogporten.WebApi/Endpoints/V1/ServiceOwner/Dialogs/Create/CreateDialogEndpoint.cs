using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Commands.Create;
using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Get;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Create;

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

        Description(b => CreateDialogSwaggerConfig.SetDescription(b));
    }

    public override async Task HandleAsync(CreateDialogCommand req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            success => SendCreatedAtAsync<GetDialogEndpoint>(new GetDialogQuery { DialogId = success.Value }, success.Value, cancellation: ct),
            domainError => this.UnprocessableEntityAsync(domainError, ct),
            validationError => this.BadRequestAsync(validationError, ct),
            forbidden => this.ForbiddenAsync(forbidden, ct));
    }
}
