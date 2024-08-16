using System.Security.Claims;
using Digdir.Domain.Dialogporten.Domain.Parties;
using Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authentication;

public class ServiceOwnerOnBehalfOfPersonMiddleware
{
    private readonly RequestDelegate _next;
    private const string EndUserId = "enduserid";
    private const string PidClaim = "pid";

    public ServiceOwnerOnBehalfOfPersonMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is not { IsAuthenticated: true })
        {
            return _next(context);
        }

        if (!context.Request.Query.TryGetValue(EndUserId, out var endUserIdQuery))
        {
            return _next(context);
        }

        if (!NorwegianPersonIdentifier.TryParse(endUserIdQuery.First(), out var endUserId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.WriteAsJsonAsync(context.ResponseBuilder(
                context.Response.StatusCode,
                [
                    new("EndUserId",
                        "EndUserId must be a valid end user identifier. It must match the format " +
                        $"'{NorwegianPersonIdentifier.PrefixWithSeparator}{{norwegian f-nr/d-nr}}'."
                    )
                ]
            ));
            return Task.CompletedTask;
        }

        OverrideClaim(context.User, PidClaim, endUserId.Id);

        return _next(context);
    }

    private static void OverrideClaim(ClaimsPrincipal claimsPrincipal, string claimType, string newClaimValue)
    {
        if (claimsPrincipal.Identity is not ClaimsIdentity identity)
        {
            throw new InvalidOperationException("ClaimsPrincipal does not have a ClaimsIdentity.");
        }

        if (!claimsPrincipal.HasClaim(claim => claim.Type == claimType))
        {
            identity.AddClaim(new Claim(claimType, newClaimValue));
            return;
        }

        foreach (var ident in claimsPrincipal.Identities)
        {
            foreach (var claim in ident.FindAll(c => c.Type == claimType))
            {
                ident.RemoveClaim(claim);
            }
        }

        identity.AddClaim(new Claim(claimType, newClaimValue));
    }
}

public static class ServiceOwnerOnBehalfOfPersonMiddlewareExtensions
{
    public static IApplicationBuilder UseServiceOwnerOnBehalfOfPerson(this IApplicationBuilder app)
        => app.UseMiddleware<ServiceOwnerOnBehalfOfPersonMiddleware>();
}
