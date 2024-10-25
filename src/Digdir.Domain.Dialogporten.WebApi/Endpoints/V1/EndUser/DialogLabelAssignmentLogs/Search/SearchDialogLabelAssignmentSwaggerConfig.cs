using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssignmentLog.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabelAssignmentLogs.Search;

public sealed class SearchDialogLabelAssignmentSwaggerConfig : ISwaggerConfig
{

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder, Type type)
        => builder.OperationId(TypeNameConverter.Convert(type)).ProducesOneOf<List<SearchDialogLabelAssignmentLogDto>>(
            StatusCodes.Status200OK,
            StatusCodes.Status404NotFound,
            StatusCodes.Status410Gone);
    public static object GetExample() => throw new NotImplementedException();
}
