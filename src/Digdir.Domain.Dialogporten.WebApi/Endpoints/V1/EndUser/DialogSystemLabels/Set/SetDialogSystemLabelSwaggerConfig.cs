using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogSystemLabels.Set;

public sealed class SetDialogSystemLabelSwaggerConfig
{
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder, Type type)
        => builder.OperationId(TypeNameConverter.Convert(type)).ProducesOneOf(
            StatusCodes.Status204NoContent,
            StatusCodes.Status400BadRequest,
            StatusCodes.Status403Forbidden,
            StatusCodes.Status404NotFound,
            StatusCodes.Status410Gone,
            StatusCodes.Status412PreconditionFailed,
            StatusCodes.Status422UnprocessableEntity);
}
