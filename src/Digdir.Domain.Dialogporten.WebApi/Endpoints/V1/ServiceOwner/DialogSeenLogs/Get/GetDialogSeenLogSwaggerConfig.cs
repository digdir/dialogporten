using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogSeenLogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogSeenLogs.Get;

public class GetDialogSeenLogSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetDialogSeenLogSO";
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId)
            .ProducesOneOf<GetDialogSeenLogDto>(
                StatusCodes.Status200OK,
                StatusCodes.Status404NotFound);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class GetDialogSeenLogEndpointSummary : Summary<GetDialogSeenLogEndpoint>
{
    public GetDialogSeenLogEndpointSummary()
    {
        Summary = "Gets a single dialog seen log record";
        Description = """
                      Gets a single dialog seen log record. For more information see the documentation (link TBD).
                      """;

        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("seen log record");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogElementNotFound;
    }
}
