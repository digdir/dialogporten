using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using Digdir.Domain.Dialogporten.Application.Common.Numbers;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    private const string ConsumerClaim = "consumer";
    private const string AuthorityClaim = "authority";
    private const string AuthorityValue = "iso6523-actorid-upis";
    private const string IdClaim = "ID";
    private const char IdDelimiter = ':';
    private const string IdPrefix = "0192";
    private const string OrgClaim = "urn:altinn:org";

    public static bool TryGetOrgNumber(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgNumber)
        => claimsPrincipal.FindFirst(ConsumerClaim).TryGetOrgNumber(out orgNumber);

    public static bool TryGetOrgNumber(this Claim? consumerClaim, [NotNullWhen(true)] out string? orgNumber)
    {
        orgNumber = null;
        if (consumerClaim is null || consumerClaim.Type != ConsumerClaim)
        {
            return false;
        }

        var consumerClaimJson = JsonSerializer.Deserialize<Dictionary<string, string>>(consumerClaim.Value);

        if (consumerClaimJson is null)
        {
            return false;
        }

        if (!consumerClaimJson.TryGetValue(AuthorityClaim, out var authority) ||
            !string.Equals(authority, AuthorityValue, StringComparison.OrdinalIgnoreCase))
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

    public static bool TryGetOrgShortName(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgShortName)
        => claimsPrincipal.FindFirst(OrgClaim).TryGetOrgShortName(out orgShortName);

    public static bool TryGetOrgShortName(this Claim? orgClaim, [NotNullWhen(true)] out string? orgShortName)
    {
        orgShortName = orgClaim?.Value;

        return orgShortName is not null;
    }

    internal static bool TryGetOrgNumber(this IUser user, [NotNullWhen(true)] out string? orgNumber) =>
        user.GetPrincipal().TryGetOrgNumber(out orgNumber);

    internal static bool TryGetOrgShortName(this IUser user, [NotNullWhen(true)] out string? orgShortName) =>
        user.GetPrincipal().TryGetOrgShortName(out orgShortName);
}
