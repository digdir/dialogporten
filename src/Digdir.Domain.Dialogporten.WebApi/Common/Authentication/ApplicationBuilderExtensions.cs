namespace Digdir.Domain.Dialogporten.WebApi.Common.Authentication;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseDialogportenAuthentication(this IApplicationBuilder app)
    {
        app.UseMiddleware<JwtSchemeSelectorMiddleware>().UseAuthentication();
        return app;
    }
}
