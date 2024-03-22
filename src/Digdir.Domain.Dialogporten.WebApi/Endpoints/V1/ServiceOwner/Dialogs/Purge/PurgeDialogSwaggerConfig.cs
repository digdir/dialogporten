using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Purge;

public class PurgeDialogSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "PurgeDialog";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId)
            .Accepts<PurgeDialogRequest>()
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class PurgeDialogEndpointSummary : Summary<PurgeDialogEndpoint>
{
    public PurgeDialogEndpointSummary()
    {
        Summary = "Permanently deletes a dialog";
        Description = """
                      Deletes a given dialog (hard delete). For more information see the documentation (link TBD).

                      Optimistic concurrency control is implemented using the If-Match header. Supply the Revision value from the GetDialog endpoint to ensure that the dialog is not deleted by another request in the meantime.
                      """;

        Responses[StatusCodes.Status204NoContent] = Constants.SwaggerSummary.Deleted.FormatInvariant("aggregate");
        Responses[StatusCodes.Status401Unauthorized] =
            Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope
                .ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] =
            Constants.SwaggerSummary.AccessDeniedToDialog.FormatInvariant("delete");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
    }
}
