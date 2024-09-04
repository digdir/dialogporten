using Digdir.Domain.Dialogporten.Infrastructure.Common.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Diagnostics;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception exception,
        CancellationToken cancellationToken)
    {
        var http = $"{ctx.Request.Scheme}: {ctx.Request.Method} {ctx.Request.Path}";
        var type = exception.GetType().Name;
        var error = exception.Message;
        var logger = ctx.Resolve<ILogger<GlobalExceptionHandler>>();
        logger.LogError(exception, "{@Http}{@Type}{@Reason}", http, type, error);
        ctx.Response.StatusCode = exception is IUpstreamServiceError
            ? StatusCodes.Status502BadGateway
            : StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/problem+json";
        await ctx.Response.WriteAsJsonAsync(ctx.ResponseBuilder(ctx.Response.StatusCode), cancellationToken);
        return true;
    }
}
