using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogActivities.Get;

public abstract class GetDialogActivitySwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetDialogActivity";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder) =>
        builder.OperationId(OperationId)
            .ProducesOneOf(
                StatusCodes.Status200OK,
                StatusCodes.Status404NotFound);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class GetDialogActivityEndpointSummary : Summary<GetDialogActivityEndpoint>
{
    public GetDialogActivityEndpointSummary()
    {
        Summary = "Gets a single dialog activity";
        Description = """
                      Gets a single activity belonging to a dialog. For more information see the documentation (link TBD).
                      """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("activity");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure;
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogActivityNotFound;
    }
}
