using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogSeenLogs.Search;

public sealed class SearchDialogSeenLogSwaggerConfig : ISwaggerConfig
{

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder, Type type)
        => builder.OperationId(TypeNameConverter.Convert(type))
            .ProducesOneOf<List<SearchSeenLogDto>>(
                StatusCodes.Status200OK,
                StatusCodes.Status404NotFound);

    public static object GetExample() => throw new NotImplementedException();
}

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
