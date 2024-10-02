using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogLabelAssignmentLog.Queries.Search;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabelAssigmentLogs.Search;

public sealed class SearchDialogLabelAssignmentSwaggerConfig : ISwaggerConfig
{

    public static string OperationId => "SearchDialogLabelAssignmentLog";
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder)
        => builder.OperationId(OperationId).ProducesOneOf<List<SearchDialogLabelAssignmentLogDto>>(
            StatusCodes.Status200OK,
            StatusCodes.Status404NotFound,
            StatusCodes.Status410Gone);
    public static object GetExample() => throw new NotImplementedException();
}
