using System.Net;
using Azure;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.Authentication;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;
using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authentication;

public class UserTypeValidationMiddleware
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
            var userType = context.User.GetUserType();
            if (userType == UserType.Unknown)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(context.ResponseBuilder(
                    context.Response.StatusCode,
                    new List<ValidationFailure>() { new("UserType",
                        "The request was authenticated, but we were unable to determine valid user type (person, enterprise or system user) in order to authorize the request.") }
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
