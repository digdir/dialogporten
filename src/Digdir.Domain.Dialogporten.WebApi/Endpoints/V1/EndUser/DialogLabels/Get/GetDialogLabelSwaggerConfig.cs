using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogLabels.Get;

public sealed class GetDialogLabelSwaggerConfig : ISwaggerConfig
{

    public static string OperationId => "GetDialogLabel";
    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder) =>
        builder.OperationId(OperationId)
            .ProducesOneOf(StatusCodes.Status200OK,
                StatusCodes.Status404NotFound);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class GetDialogLabelEndpointSummary : Summary<GetDialogLabelEndpoint>
{
    public GetDialogLabelEndpointSummary()
    {
        Summary = "Gets all the labels of a single dialog.";
        Description = """
                      TBD
                      """;
        // Amund: FormatInvariant??
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("label");
    }
}
