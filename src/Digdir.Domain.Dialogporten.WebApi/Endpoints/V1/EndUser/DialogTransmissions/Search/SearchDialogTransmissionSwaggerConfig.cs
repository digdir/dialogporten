using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogTransmissions.Search;

public sealed class SearchDialogTransmissionSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetDialogTransmissionList";
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class SearchDialogTransmissionEndpointSummary : Summary<SearchDialogTransmissionEndpoint, SearchDialogTransmissionQuery>
{
    public SearchDialogTransmissionEndpointSummary()
    {
        Summary = "Gets a list of dialog transmissions";
        Description = """
                      Gets the list of transmissions belonging to a dialog
                      """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("transmission list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure.FormatInvariant(AuthorizationScope.EndUser);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
    }
}
