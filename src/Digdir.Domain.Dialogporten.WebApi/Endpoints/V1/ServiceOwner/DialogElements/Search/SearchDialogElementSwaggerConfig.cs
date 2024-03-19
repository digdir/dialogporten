using Digdir.Domain.Dialogporten.Application.Features.V1.ServiceOwner.DialogElements.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
<<<<<<< HEAD:src/Digdir.Domain.Dialogporten.WebApi/Endpoints/V1/ServiceOwner/DialogElements/SearchDialogElementEndpoint.cs
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
=======
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
>>>>>>> 96c78f7 (refac):src/Digdir.Domain.Dialogporten.WebApi/Endpoints/V1/ServiceOwner/DialogElements/Search/SearchDialogElementSwaggerConfig.cs
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElements.Search;

public class SearchDialogElementSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "GetDialogElementListSO";

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
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("get");
    }
}
