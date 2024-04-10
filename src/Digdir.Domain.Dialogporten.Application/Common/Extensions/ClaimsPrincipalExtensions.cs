using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Digdir.Domain.Dialogporten.Domain.Parties;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    private const string ConsumerClaim = "consumer";
    private const string SupplierClaim = "supplier";
    private const string AuthorityClaim = "authority";
    private const string AuthorityValue = "iso6523-actorid-upis";
    private const string IdClaim = "ID";
    private const char IdDelimiter = ':';
    private const string IdPrefix = "0192";
    private const string AltinnClaimPrefix = "urn:altinn:";
    private const string OrgClaim = "urn:altinn:org";
    private const string IdportenAuthLevelClaim = "acr";
    private const string AltinnAuthLevelClaim = "urn:altinn:authlevel";
    private const string PidClaim = "pid";

    public static bool TryGetClaimValue(this ClaimsPrincipal claimsPrincipal, string claimType, [NotNullWhen(true)] out string? value)
    {
        value = claimsPrincipal.FindFirst(claimType)?.Value;
        return value is not null;
    }

    public static bool TryGetOrgNumber(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgNumber)
        => claimsPrincipal.FindFirst(ConsumerClaim).TryGetOrgNumber(out orgNumber);

    public static bool TryGetSupplierOrgNumber(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgNumber)
        => claimsPrincipal.FindFirst(SupplierClaim).TryGetOrgNumber(out orgNumber);

    public static bool TryGetPid(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? pid)
        => claimsPrincipal.FindFirst(PidClaim).TryGetPid(out pid);

    public static bool TryGetPid(this Claim? pidClaim, [NotNullWhen(true)] out string? pid)
    {
        pid = null;
        if (pidClaim is null || pidClaim.Type != PidClaim)
        {
            return false;
        }

        if (NorwegianPersonIdentifier.IsValid(pidClaim.Value))
        {
            pid = pidClaim.Value;
        }

        return pid is not null;
    }

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
        [IdPrefix, var on] => NorwegianOrganizationIdentifier.IsValid(on) ? on : null,
            _ => null
        };

        return orgNumber is not null;
    }

    public static bool TryGetAuthenticationLevel(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out int? authenticationLevel)
    {
        foreach (var claimType in new[] { IdportenAuthLevelClaim, AltinnAuthLevelClaim })
        {
            if (!claimsPrincipal.TryGetClaimValue(claimType, out var claimValue)) continue;
            // The acr claim value is "LevelX" where X is the authentication level
            var valueToParse = claimType == IdportenAuthLevelClaim ? claimValue[5..] : claimValue;
            if (!int.TryParse(valueToParse, out var level)) continue;

            authenticationLevel = level;
            return true;
        }

        authenticationLevel = null;
        return false;
    }

    public static IEnumerable<Claim> GetIdentifyingClaims(this List<Claim> claims) =>
        claims.Where(c =>
            c.Type == PidClaim ||
            c.Type == ConsumerClaim ||
            c.Type == SupplierClaim ||
            c.Type == OrgClaim ||
            c.Type == IdportenAuthLevelClaim ||
            c.Type.StartsWith(AltinnClaimPrefix, StringComparison.Ordinal)
        ).OrderBy(c => c.Type);

    private static bool TryGetOrgShortName(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgShortName)
        => claimsPrincipal.FindFirst(OrgClaim).TryGetOrgShortName(out orgShortName);

    private static bool TryGetOrgShortName(this Claim? orgClaim, [NotNullWhen(true)] out string? orgShortName)
    {
        orgShortName = orgClaim?.Value;

        return orgShortName is not null;
    }

    internal static bool TryGetOrgNumber(this IUser user, [NotNullWhen(true)] out string? orgNumber) =>
        user.GetPrincipal().TryGetOrgNumber(out orgNumber);

    internal static bool TryGetOrgShortName(this IUser user, [NotNullWhen(true)] out string? orgShortName) =>
        user.GetPrincipal().TryGetOrgShortName(out orgShortName);

    internal static bool TryGetPid(this IUser user, [NotNullWhen(true)] out string? pid) =>
        user.GetPrincipal().TryGetPid(out pid);
}
