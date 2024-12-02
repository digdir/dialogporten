using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.DialogTransmissions.Create;

public sealed class CreateDialogTransmissionEndpointSummary : Summary<CreateDialogTransmissionEndpoint>
{
    public CreateDialogTransmissionEndpointSummary()
    {
        Summary = "Adds a transmission to a dialog";
        Description = $"""
                       The transmission is created with the given configuration. For more information see the documentation (link TBD).

                       {Constants.SwaggerSummary.OptimisticConcurrencyNote}
                       """;

        ResponseExamples[StatusCodes.Status201Created] = "018bb8e5-d9d0-7434-8ec5-569a6c8e01fc";

        Responses[StatusCodes.Status201Created] = Constants.SwaggerSummary.Created.FormatInvariant("transmission");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("create");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogNotFound;
        Responses[StatusCodes.Status410Gone] = Constants.SwaggerSummary.DialogDeleted;
        Responses[StatusCodes.Status412PreconditionFailed] = Constants.SwaggerSummary.RevisionMismatch;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
