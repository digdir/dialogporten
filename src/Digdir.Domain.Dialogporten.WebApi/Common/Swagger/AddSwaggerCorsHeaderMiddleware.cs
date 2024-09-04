namespace Digdir.Domain.Dialogporten.WebApi.Common.Swagger;

public class AddSwaggerCorsHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public AddSwaggerCorsHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger/v1/swagger.json"))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
        }
        return _next(context);
    }
}

public static class AddSwaggerCorsHeaderMiddlewareExtensions
{
    public static IApplicationBuilder UseAddSwaggerCorsHeader(this IApplicationBuilder app)
        => app.UseMiddleware<AddSwaggerCorsHeaderMiddleware>();
}
