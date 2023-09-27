using Digdir.Domain.Dialogporten.Application.Common.Numbers;
using Microsoft.AspNetCore.Authorization;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authorization;

internal static class AuthorizationPolicyBuilderExtensions
{
    private const string ScopeClaim = "scope";
    private const char ScopeClaimSeparator = ' ';
    private const string ConsumerClaim = "consumer";
    private const string AuthorityClaim = "authority";
    private const string AuthorityValue = "iso6523-actorid-upis";
    private const string IdClaim = "ID";
    private const char IdDelimiter = ':';
    private const string IdPrefix = "0192";

    public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder builder, string scope) =>
        builder.RequireAssertion(ctx => ctx.User.Claims
            .Where(x => x.Type == ScopeClaim)
            .Select(x => x.Value)
            .Any(scopeValue => scopeValue == scope || scopeValue
                .Split(ScopeClaimSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Contains(scope)));

    public static AuthorizationPolicyBuilder RequireValidConsumerClaim(this AuthorizationPolicyBuilder builder)
    {
        return builder.RequireAssertion(x =>
         {
             var conumerClaim = x.User.FindFirst(ConsumerClaim);
             if (conumerClaim is null)
             {
                 return false;
             }

             if (!conumerClaim.Properties.TryGetValue(AuthorityClaim, out var authority) ||
                 authority != AuthorityValue)
             {
                 return false;
             }

             if (!conumerClaim.Properties.TryGetValue(IdClaim, out var id))
             {
                 return false;
             }

             return id.Split(IdDelimiter) switch
             {
                 [IdPrefix, var orgNumber] => OrganizationNumber.IsValid(orgNumber),
                 _ => false
             };
         });
    }
}