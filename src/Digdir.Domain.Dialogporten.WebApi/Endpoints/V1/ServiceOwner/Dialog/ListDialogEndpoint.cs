using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.ServiceOwner.List;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialog;

public class ListDialogEndpoint : Endpoint<ListDialogQuery>
{
    private readonly ISender _sender;

    public ListDialogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs");
        Group<ServiceOwnerGroup>();
    }

    public override async Task HandleAsync(ListDialogQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            paginatedDto => SendOkAsync(paginatedDto, ct),
            validationError => this.BadRequestAsync(validationError, ct));
    }
}
