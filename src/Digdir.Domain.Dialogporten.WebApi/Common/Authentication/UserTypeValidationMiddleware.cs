using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using UserIdType = Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogUserType.Values;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authentication;

public sealed class UserTypeValidationMiddleware
{
    private readonly RequestDelegate _next;

    public UserTypeValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var (userType, _) = context.User.GetUserType();
            if (userType == UserIdType.Unknown)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(context.ResponseBuilder(
                    context.Response.StatusCode,
                    [
                        new("Type",
                            "The request was authenticated, but we were unable to determine valid user type (person or system user) in order to authorize the request.")
                    ]
                ));

                return;
            }
        }

        await _next(context);
    }
}

public static class UserTypeValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseUserTypeValidation(this IApplicationBuilder app)
        => app.UseMiddleware<UserTypeValidationMiddleware>();
}
