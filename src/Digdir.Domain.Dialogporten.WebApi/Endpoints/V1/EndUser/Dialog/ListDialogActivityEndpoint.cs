using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogActivities.Queries.List;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.Dialog;

public class ListDialogActivityEndpoint : Endpoint<ListDialogActivityQuery>
{
    private readonly ISender _sender;

    public ListDialogActivityEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/activities");
        Group<EndUserGroup>();
    }

    public override async Task HandleAsync(ListDialogActivityQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct));
    }
}