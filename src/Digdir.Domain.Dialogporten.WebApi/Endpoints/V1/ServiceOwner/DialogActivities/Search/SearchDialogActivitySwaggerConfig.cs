using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogActivities.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
<<<<<<< HEAD:src/Digdir.Domain.Dialogporten.WebApi/Endpoints/V1/ServiceOwner/DialogActivities/SearchDialogActivityEndpoint.cs
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
=======
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
>>>>>>> 96c78f7 (refac):src/Digdir.Domain.Dialogporten.WebApi/Endpoints/V1/ServiceOwner/DialogActivities/Search/SearchDialogActivitySwaggerConfig.cs
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogActivities.Search;

public class SearchDialogActivitySwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetDialogActivityListSO";
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class SearchDialogActivityEndpointSummary : Summary<SearchDialogActivityEndpoint, SearchDialogActivityQuery>
{
    public SearchDialogActivityEndpointSummary()
    {
        Summary = "Gets a list of dialog activities";
        Description = """
                      Gets the list of activities belonging to a dialog
                      """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("activity list");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
    }
}
