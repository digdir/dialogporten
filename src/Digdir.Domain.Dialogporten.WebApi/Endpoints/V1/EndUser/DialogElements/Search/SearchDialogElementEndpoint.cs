using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogElements.Search;

public class SearchDialogElementEndpoint : Endpoint<SearchDialogElementQuery>
{
    private readonly ISender _sender;

    public SearchDialogElementEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/elements");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(b => SearchDialogElementSwaggerConfig.SetDescription(b));
    }

    public override async Task HandleAsync(SearchDialogElementQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct));
    }
}
