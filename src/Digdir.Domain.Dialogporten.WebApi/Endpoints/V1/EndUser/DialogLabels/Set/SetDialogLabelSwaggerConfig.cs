using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabels.Set;

public sealed class SetDialogLabelSwaggerConfig
{
    public static string OperationId => "SetDialogLabel";
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder) => builder.OperationId(OperationId).ProducesOneOf(
        StatusCodes.Status204NoContent,
        StatusCodes.Status400BadRequest,
        StatusCodes.Status403Forbidden,
        StatusCodes.Status404NotFound,
        StatusCodes.Status410Gone,
        StatusCodes.Status412PreconditionFailed,
        StatusCodes.Status422UnprocessableEntity);
}
