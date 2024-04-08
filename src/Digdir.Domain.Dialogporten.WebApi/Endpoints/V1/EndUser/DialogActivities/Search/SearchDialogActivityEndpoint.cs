using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogActivities.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogActivities.Search;

public class SearchDialogActivityEndpoint : Endpoint<SearchDialogActivityQuery>
{
    private readonly ISender _sender;

    public SearchDialogActivityEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/activities");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(b => SearchDialogActivitySwaggerConfig.SetDescription(b));
    }

    public override async Task HandleAsync(SearchDialogActivityQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct));
    }
}
