using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Get;

public sealed class GetDialogEndpointSummary : Summary<GetDialogEndpoint>
{
    public GetDialogEndpointSummary()
    {
        Summary = "Gets a single dialog";
        Description = """
                      Gets a single dialog aggregate. For more information see the documentation (link TBD).

                      Note that this operation may return deleted dialogs (see the field `DeletedAt`).
                      """;

        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("aggregate");
        Responses[StatusCodes.Status401Unauthorized] =
            Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope
                .ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] =
            Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
    }
}
