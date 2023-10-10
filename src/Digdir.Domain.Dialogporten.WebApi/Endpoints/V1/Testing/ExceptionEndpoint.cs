using Digdir.Domain.Dialogporten.WebApi.Common.Authorization;
using FastEndpoints;

namespace Digdir.Domain.Dialogporten.WebApi.Endpoints.V1.Testing;

public sealed class ExceptionEndpoint : Endpoint<ExceptionEndpointRequest>
{
    public override void Configure()
    {
        Get("Exception");
        Policies(AuthorizationPolicy.Testing);
        Description(x => x.ExcludeFromDescription());
    }

    public override Task HandleAsync(ExceptionEndpointRequest request, CancellationToken ct)
    {
        var exceptionType = Type.GetType(request.ExceptionType);

        if (exceptionType is null || !exceptionType.IsAssignableTo(typeof(Exception)))
        {
            throw new Exception(request.ExceptionMessage);
        }

        throw (Exception)Activator.CreateInstance(exceptionType, request.ExceptionMessage)!;
    }
}

public sealed class ExceptionEndpointRequest
{
    public string? ExceptionMessage { get; init; }
    public string ExceptionType { get; init; } = "Exception";
}
