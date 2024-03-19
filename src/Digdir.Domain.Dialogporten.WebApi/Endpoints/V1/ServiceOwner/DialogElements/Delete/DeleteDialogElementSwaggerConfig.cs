using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogElements.Delete;

public class DeleteDialogElementSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "DeleteDialogElement";

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

public sealed class DeleteDialogElementEndpointSummary : Summary<DeleteDialogElementEndpoint>
{
    public DeleteDialogElementEndpointSummary()
    {
        Summary = "Deletes a dialog element";
        Description = $"""
                       Deletes a given dialog element (hard delete). For more information see the documentation (link TBD).

                       {Constants.SwaggerSummary.OptimisticConcurrencyNote}
                       """;
        Responses[StatusCodes.Status204NoContent] = Constants.SwaggerSummary.Deleted.FormatInvariant("element");
        Responses[StatusCodes.Status401Unauthorized] =
            Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope
                .ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] =
            Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("delete");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogElementNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
    }
}
