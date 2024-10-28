using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogSeenLogs.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogSeenLogs.Get;

public sealed class GetDialogSeenLogSwaggerConfig : ISwaggerConfig
{
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder, Type type)
        => builder.OperationId(TypeNameConverter.ToShortNameStrict(type))
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
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure;
    }
}
