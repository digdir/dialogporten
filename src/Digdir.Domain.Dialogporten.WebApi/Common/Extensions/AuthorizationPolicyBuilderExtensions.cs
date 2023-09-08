using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Extensions;

internal static class AuthorizationPolicyBuilderExtensions
{
    private const string Scope = "scope";
    private const char ScopeSeparator = ' ';

    public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder builder, string scope) =>
        builder.RequireAssertion(ctx => ctx.User.Claims
            .Where(x => x.Type == Scope)
            .Select(x => x.Value)
            .Any(scopeValue => scopeValue == scope || scopeValue
                .Split(ScopeSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Contains(scope)));
}