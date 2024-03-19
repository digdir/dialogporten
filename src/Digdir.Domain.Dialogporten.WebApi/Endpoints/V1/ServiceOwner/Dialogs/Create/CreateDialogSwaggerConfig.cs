using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.ServiceOwner.Dialogs.Create;

public class CreateDialogSwaggerConfig : ISwaggerConfig
{
    public static string OperationId => "CreateDialog";

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId)
            .ProducesOneOf(
                StatusCodes.Status201Created,
                StatusCodes.Status400BadRequest,
                StatusCodes.Status422UnprocessableEntity);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class CreateDialogEndpointSummary : Summary<CreateDialogEndpoint>
{
    public CreateDialogEndpointSummary()
    {
        Summary = "Creates a new dialog";
        Description = """
                      The dialog is created with the given configuration. For more information see the documentation (link TBD).

                      For detailed information on validation rules, see [the source for CreateDialogCommandValidator](https://github.com/digdir/dialogporten/blob/main/src/Digdir.Domain.Dialogporten.Application/Features/V1/ServiceOwner/Dialogs/Commands/Create/CreateDialogCommandValidator.cs)
                      """;

        ResponseExamples[StatusCodes.Status201Created] = "018bb8e5-d9d0-7434-8ec5-569a6c8e01fc";

        Responses[StatusCodes.Status201Created] = Constants.SwaggerSummary.Created.FormatInvariant("aggregate");
        Responses[StatusCodes.Status400BadRequest] = Constants.SwaggerSummary.ValidationError;
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.ServiceOwnerAuthenticationFailure.FormatInvariant(AuthorizationScope.ServiceProvider);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.DialogCreationNotAllowed;
        Responses[StatusCodes.Status422UnprocessableEntity] = Constants.SwaggerSummary.DomainError;
    }
}
