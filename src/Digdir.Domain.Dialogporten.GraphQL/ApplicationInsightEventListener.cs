using System.Globalization;
using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Digdir.Domain.Dialogporten.GraphQL;

public class ApplicationInsightEventListener : ExecutionDiagnosticEventListener
{
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightEventListener(
        TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public override IDisposable ExecuteRequest(IRequestContext context)
    {
        var httpContext = GetHttpContextFrom(context);
        if (httpContext == null)
            return EmptyScope;

        //During debugging every playground action will come here, so we want this while debugging
#if DEBUG
        if (context.Request.OperationName == "IntrospectionQuery")
            return EmptyScope;
#endif

        //Create a new telemetry request
        var operationPath = $"{context.Request.OperationName ?? "UnknownOperation"} - {context.Request.QueryHash}";
        var requestTelemetry = new RequestTelemetry
        {
            Name = $"/graphql{operationPath}",
            Url = new Uri(httpContext.Request.GetUri().AbsoluteUri + operationPath)
        };

        requestTelemetry.Context.Operation.Name = $"POST /graphql/{operationPath}";
        requestTelemetry.Context.Operation.Id = GetOperationIdFrom(httpContext);
        requestTelemetry.Context.Operation.ParentId = GetOperationIdFrom(httpContext);
        requestTelemetry.Context.User.AuthenticatedUserId = httpContext.User.Identity?.Name ?? "Not authenticated";

        var operation = _telemetryClient.StartOperation(requestTelemetry);
        return new ScopeWithEndAction(() => OnEndRequest(context, operation));
    }

    public override void RequestError(IRequestContext context, Exception exception)
    {
        _telemetryClient.TrackException(exception);
        base.RequestError(context, exception);
    }

    public override void ValidationErrors(IRequestContext context, IReadOnlyList<IError> errors)
    {
        foreach (var error in errors)
        {
            _telemetryClient.TrackTrace("GraphQL validation error: " + error.Message, SeverityLevel.Warning);
        }

        base.ValidationErrors(context, errors);
    }

    private static HttpContext? GetHttpContextFrom(IRequestContext context) =>
        // This method is used to enable start/stop events for query.
        !context.ContextData.TryGetValue("HttpContext", out var value) ? null : value as HttpContext;

    private static string GetOperationIdFrom(HttpContext context) => context.TraceIdentifier;

    private void OnEndRequest(IRequestContext context, IOperationHolder<RequestTelemetry> operation)
    {
        var httpContext = GetHttpContextFrom(context);
        operation.Telemetry.Success = httpContext is { Response.StatusCode: >= 200 and <= 299 };
        if (httpContext != null)
            operation.Telemetry.ResponseCode = httpContext.Response.StatusCode.ToString(CultureInfo.InvariantCulture);

        if (context.Exception != null)
        {
            operation.Telemetry.Success = false;
            operation.Telemetry.ResponseCode = "500";
            _telemetryClient.TrackException(context.Exception);
        }

        if (context.Result is QueryResult { Errors: not null } queryResult)
        {
            foreach (var error in queryResult.Errors)
            {
                if (error.Exception is null)
                {
                    continue;
                }

                operation.Telemetry.Success = false;
                _telemetryClient.TrackException(error.Exception);
            }
        }

        _telemetryClient.StopOperation(operation);
    }
}

internal sealed class ScopeWithEndAction : IDisposable
{
    private readonly Action _disposeAction;

    public ScopeWithEndAction(Action disposeAction)
    {
        _disposeAction = disposeAction;
    }

    public void Dispose() => _disposeAction.Invoke();
}
