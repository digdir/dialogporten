using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogSeenLogs.Search;

public sealed class SearchDialogSeenLogEndpoint : Endpoint<SearchDialogSeenLogQuery, List<SeenLogDto>>
{
    private readonly ISender _sender;

    public SearchDialogSeenLogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/seenlog");
        Policies(AuthorizationPolicy.ServiceProvider);
        Group<ServiceOwnerGroup>();

        Description(d => d.ProducesOneOf<List<SeenLogDto>>(
            StatusCodes.Status200OK,
            StatusCodes.Status404NotFound));
    }

    public override async Task HandleAsync(SearchDialogSeenLogQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct));
    }
}
