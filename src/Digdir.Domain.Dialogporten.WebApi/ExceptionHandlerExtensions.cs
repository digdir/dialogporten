using FastEndpoints;
using Microsoft.AspNetCore.Diagnostics;

namespace Digdir.Domain.Dialogporten.WebApi;

internal static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseProblemDetailsExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async ctx =>
            {
                var exHandlerFeature = ctx.Features.Get<IExceptionHandlerFeature>();
                if (exHandlerFeature is null)
                {
                    return;
                }

                var http = exHandlerFeature.Endpoint?.DisplayName?.Split(" => ")[0];
                var type = exHandlerFeature.Error.GetType().Name;
                var error = exHandlerFeature.Error.Message;
                var logger = ctx.Resolve<ILogger<ExceptionHandler>>();
                logger.LogError("{@http}{@type}{@reason}", http, type, error);
                // TODO: Logger will print forever if error comes from ef core / postgre. Figgure it out and print error again
                //logger.LogError("{@http}{@type}{@reason}{@exception}", http, type, error, exHandlerFeature.Error);
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = "application/problem+json";
                await ctx.Response.WriteAsJsonAsync(ctx.ResponseBuilder(ctx.Response.StatusCode));
            });
        });

        return app;
    }

    private class ExceptionHandler { }
}