using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Commands.Create;
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
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(CreateDialogCommand req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            id => SendCreatedAtAsync<GetDialogEndpoint>(new { id }, id, cancellation: ct),
            entityExists => this.ConflictAsync(entityExists, ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}
