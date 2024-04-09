using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogSeenLogs.Get;

public class GetDialogSeenLogEndpoint : Endpoint<GetDialogSeenLogQuery, GetDialogSeenLogDto>
{
    private readonly ISender _sender;

    public GetDialogSeenLogEndpoint(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public override void Configure()
    {
        Get("dialogs/{dialogId}/seenlog/{seenLogId}");
        Policies(AuthorizationPolicy.EndUser);
        Group<EndUserGroup>();

        Description(d => GetDialogSeenLogSwaggerConfig.SetDescription(d));
    }

    public override async Task HandleAsync(GetDialogSeenLogQuery req, CancellationToken ct)
    {
        var result = await _sender.Send(req, ct);
        await result.Match(
            dto => SendOkAsync(dto, ct),
            notFound => this.NotFoundAsync(notFound, ct),
            deleted => this.GoneAsync(deleted, ct),
            forbiden => this.ForbiddenAsync(forbiden, ct));
    }
}
