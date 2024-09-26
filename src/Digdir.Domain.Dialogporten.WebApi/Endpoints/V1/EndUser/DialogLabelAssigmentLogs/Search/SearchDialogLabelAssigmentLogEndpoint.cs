using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssigmentLog.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabelAssigmentLogs.Search;

public sealed class SearchDialogLabelAssigmentLogEndpoint : Endpoint<SearchDialogLabelAssignmentLogQuery, List<SearchDialogLabelAssignmentLogDto>>
{
    private readonly ISender _sender;

    public SearchDialogLabelAssigmentLogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }
    public override void Configure()
    {
        Get("dialogs/{dialogId}/labels");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(d => SearchDialogLabelAssignmentSwaggerConfig.SetDescription(d));
    }
    public override async Task HandleAsync(SearchDialogLabelAssignmentLogQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct),
            forbidden => this.ForbiddenAsync(forbidden, ct));
    }
}
