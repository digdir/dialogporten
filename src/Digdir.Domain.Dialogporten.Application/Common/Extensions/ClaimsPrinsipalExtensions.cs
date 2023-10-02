using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Common.Numbers;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class ClaimsPrinsipalExtensions
{
    private const string ConsumerClaim = "consumer";
    private const string AuthorityClaim = "authority";
    private const string AuthorityValue = "iso6523-actorid-upis";
    private const string IdClaim = "ID";
    private const char IdDelimiter = ':';
    private const string IdPrefix = "0192";

    public static bool TryGetOrgNumber(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgNumber)
    {
        orgNumber = null;
        var consumerClaim = claimsPrincipal.FindFirst(ConsumerClaim);

        if (consumerClaim is null)
        {
            return false;
        }

        var consumerClaimJson = JsonSerializer.Deserialize<Dictionary<string, string>>(consumerClaim.Value);

        if (consumerClaimJson is null)
        {
            return false;
        }

        if (!consumerClaimJson.TryGetValue(AuthorityClaim, out var authority) ||
            string.Equals(authority, AuthorityValue, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        if (!consumerClaimJson.TryGetValue(IdClaim, out var id))
        {
            return false;
        }

        orgNumber = id.Split(IdDelimiter) switch
        {
            [IdPrefix, var on] => OrganizationNumber.IsValid(on) ? on : null,
            _ => null
        };

        return orgNumber is not null;
    }

    internal static bool TryGetOrgNumber(this IUser user, [NotNullWhen(true)] out string? orgNumber) =>
        user.GetPrincipal().TryGetOrgNumber(out orgNumber);
}