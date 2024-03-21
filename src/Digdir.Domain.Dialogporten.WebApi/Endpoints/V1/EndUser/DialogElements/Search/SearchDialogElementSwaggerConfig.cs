using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogElements.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogElements.Search;

public class SearchDialogElementSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetDialogElementList";
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class SearchDialogElementEndpointSummary : Summary<SearchDialogElementEndpoint, SearchDialogElementQuery>
{
    public SearchDialogElementEndpointSummary()
    {
        Summary = "Gets a list of dialog elements";
        Description = """
                      Gets the list of elements belonging to a dialog
                      """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("element list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure;
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("get");
    }
}
