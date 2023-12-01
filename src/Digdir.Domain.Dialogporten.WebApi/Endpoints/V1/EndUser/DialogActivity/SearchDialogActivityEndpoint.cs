using System.Globalization;
using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogActivities.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;
using MediatR;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogActivity;

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
        Group<EndUserGroup>();

        Description(b => b
            .OperationId("GetDialogActivityList")
        );
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

public sealed class SearchDialogActivityEndpointSummary : Summary<SearchDialogActivityEndpoint, SearchDialogActivityQuery>
{
    public SearchDialogActivityEndpointSummary()
    {
        Summary = "Gets a list of dialog activities";
        Description = """
                Gets the list of activities belonging to a dialog
                """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.Format("activity list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure;
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.Format("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
    }
}
