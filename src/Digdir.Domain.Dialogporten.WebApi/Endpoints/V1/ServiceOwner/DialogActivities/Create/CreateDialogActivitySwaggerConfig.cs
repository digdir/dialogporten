using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogActivities.Create;

public class CreateDialogActivitySwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "CreateDialogActivity";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId("CreateDialogActivity")
            .ProducesOneOf(
                StatusCodes.Status201Created,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status404NotFound,
                StatusCodes.Status412PreconditionFailed,
                StatusCodes.Status422UnprocessableEntity);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class CreateDialogActivityEndpointSummary : Summary<CreateDialogActivityEndpoint>
{
    public CreateDialogActivityEndpointSummary()
    {
        Summary = "Adds a activity to a dialogs activity history";
        Description = $"""
                       The activity is created with the given configuration. For more information see the documentation (link TBD).

                       {Constants.SwaggerSummary.OptimisticConcurrencyNote}
                       """;

        ResponseExamples[StatusCodes.Status201Created] = "018bb8e5-d9d0-7434-8ec5-569a6c8e01fc";

        Responses[StatusCodes.Status201Created] = Constants.SwaggerSummary.Created.FormatInvariant("activity");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("create");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
