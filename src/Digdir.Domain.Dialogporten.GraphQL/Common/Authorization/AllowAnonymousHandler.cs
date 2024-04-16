using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;

/// <summary>
/// This authorisation handler will bypass all requirements
/// </summary>
public class AllowAnonymousHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (var requirement in context.PendingRequirements)
        {
            //Simply pass all requirements
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
