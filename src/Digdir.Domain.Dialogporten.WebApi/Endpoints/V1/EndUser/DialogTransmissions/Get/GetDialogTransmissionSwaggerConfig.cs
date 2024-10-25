using Digdir.Domain.Dialogporten.Application.Features.V1.EndUser.DialogTransmissions.Queries.Get;
using Digdir.Domain.Dialogporten.WebApi.Common;
using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Swagger;
using Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Common.Extensions;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.EndUser.DialogTransmissions.Get;

public sealed class GetDialogTransmissionSwaggerConfig : ISwaggerConfig
{

    public static RouteHandlerBuilder SetDescription(RouteHandlerBuilder builder, Type type)
        => builder.OperationId(TypeNameConverter.Convert(type))
            .ProducesOneOf<GetDialogTransmissionDto>(
                StatusCodes.Status200OK,
                StatusCodes.Status404NotFound);

    public static object GetExample() => throw new NotImplementedException();
}

public sealed class GetDialogTransmissionEndpointSummary : Summary<GetDialogTransmissionEndpoint>
{
    public GetDialogTransmissionEndpointSummary()
    {
        Summary = "Gets a single dialog transmission";
        Description = """
                      Gets a single transmission belonging to a dialog. For more information see the documentation (link TBD).
                      """;
        Responses[StatusCodes.Status200OK] = Constants.SwaggerSummary.ReturnedResult.FormatInvariant("transmission");
        Responses[StatusCodes.Status401Unauthorized] = Constants.SwaggerSummary.EndUserAuthenticationFailure.FormatInvariant(AuthorizationScope.EndUser);
        Responses[StatusCodes.Status403Forbidden] = Constants.SwaggerSummary.AccessDeniedToDialogForChildEntity.FormatInvariant("get");
        Responses[StatusCodes.Status404NotFound] = Constants.SwaggerSummary.DialogTransmissionNotFound;
    }
}
