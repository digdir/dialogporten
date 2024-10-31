using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogSeenLogs.Search;

public sealed class SearchDialogSeenLogEndpointSummary : Summary<SearchDialogSeenLogEndpoint>
{
    public SearchDialogSeenLogEndpointSummary()
    {
        const string summary = "Gets all seen log records for a dialog";
        Summary = summary;
        Description = $"""
                      {summary}. For more information see the documentation (link TBD).
                      """;

        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("seen log records");
    }
}
