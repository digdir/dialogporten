using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssignmentLog.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabelAssignmentLogs.Search;

public sealed class SearchDialogLabelAssignmentLogEndpoint : Endpoint<SearchDialogLabelAssignmentLogQuery, List<DialogLabelAssignmentLogDto>>
{
    private readonly ISender _sender;

    public SearchDialogLabelAssignmentLogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }
    public override void Configure()
    {
        Get("dialogs/{dialogId}/labellog");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(d => SearchDialogLabelAssignmentSwaggerConfig.SetDescription(d, GetType()));
    }
    public override async Task HandleAsync(SearchDialogLabelAssignmentLogQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct));
    }
}
