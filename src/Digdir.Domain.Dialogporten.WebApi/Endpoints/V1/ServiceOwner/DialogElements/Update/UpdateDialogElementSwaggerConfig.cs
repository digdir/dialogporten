using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElements.Update;

public class UpdateDialogElementSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "ReplaceDialogElement";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId)
            .ProducesOneOf(
                StatusCodes.Status204NoContent,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed,
                StatusCodes.Status422UnprocessableEntity);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class UpdateDialogElementEndpointSummary : Summary<UpdateDialogElementEndpoint>
{
    public UpdateDialogElementEndpointSummary()
    {
        Summary = "Replaces a dialog element";
        Description = """
                      Replaces a given dialog element with the supplied model. For more information see the documentation (link TBD).

                      Optimistic concurrency control is implemented using the If-Match header. Supply the Revision value from the GetDialog endpoint to ensure that the dialog is not deleted by another request in the meantime.
                      """;
        Responses[StatusCodes.Status204NoContent] = Constants.SwaggerSummary.Updated.FormatInvariant("element");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] =
            Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope
                .ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] =
            Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("update");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogElementNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
