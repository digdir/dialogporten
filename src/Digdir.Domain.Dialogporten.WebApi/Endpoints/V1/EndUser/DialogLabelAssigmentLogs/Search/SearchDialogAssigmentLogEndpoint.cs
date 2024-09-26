using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssigmentLog.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabelAssigmentLogs.Search;

public class SearchDialogAssigmentLogEndpoint : Endpoint<SearchDialogLabelAssignmentLogQuery, List<SearchDialogLabelAssignmentLogDto>>
{
    private readonly ISender _sender;

    public SearchDialogAssigmentLogEndpoint(ISender sender)
    {
        _sender = sender;
    }
    public override void Configure()
    {
        Get("dialogd/{dialogId}/labels");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(d => SearchDialogLabelAssignmentSwaggerConfig.SetDescription(d));
    }
}
