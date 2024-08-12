﻿using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using System.Security.Claims;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Domain.Parties;
using UserIdType = Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogUserType.Values;

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
    private const string IdportenAuthLevelClaim = "acr";
    private const string AltinnAutorizationDetailsClaim = "authorization_details";
    private const string AttributeIdSystemUser = "urn:altinn:systemuser";
    private const string AltinnAuthLevelClaim = "urn:altinn:authlevel";
    private const string AltinnAuthenticationMethodClaim = "urn:altinn:authenticatemethod";
    private const string AltinnAuthenticationEnterpriseUserMethod = "virksomhetsbruker";
    private const string AltinnUserIdClaim = "urn:altinn:userid";
    private const string AltinnUserNameClaim = "urn:altinn:username";
    private const string ScopeClaim = "scope";
    private const char ScopeClaimSeparator = ' ';
    private const string PidClaim = "pid";


    // TODO: This scope is also defined in WebAPI/GQL. Can this be fetched from a common auth lib?
    // https://github.com/digdir/dialogporten/issues/647
    // This could be done for all claims/scopes/prefixes etc, there are duplicates
    public const string ServiceProviderScope = "digdir:dialogporten.serviceprovider";

    public static bool TryGetClaimValue(this ClaimsPrincipal claimsPrincipal, string claimType,
        [NotNullWhen(true)] out string? value)
    {
        value = claimsPrincipal.FindFirst(claimType)?.Value;
        return value is not null;
    }

    public static bool TryGetOrganizationNumber(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgNumber)
        => claimsPrincipal.FindFirst(ConsumerClaim).TryGetOrganizationNumber(out orgNumber);

    public static bool HasScope(this ClaimsPrincipal claimsPrincipal, string scope) =>
        claimsPrincipal.TryGetClaimValue(ScopeClaim, out var scopes) &&
        scopes.Split(ScopeClaimSeparator).Contains(scope);

    public static bool TryGetSupplierOrgNumber(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? orgNumber)
        => claimsPrincipal.FindFirst(SupplierClaim).TryGetOrganizationNumber(out orgNumber);

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

    // https://docs.altinn.studio/authentication/systemauthentication/
    private class SystemUserAuthorizationDetails
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("systemuser_id")]
        public string[]? SystemUserIds { get; set; }
    }

    private static bool TryGetAuthorizationDetailsClaimValue(this ClaimsPrincipal claimsPrincipal,
        [NotNullWhen(true)] out SystemUserAuthorizationDetails[]? authorizationDetails)
    {
        authorizationDetails = null;

        if (!claimsPrincipal.TryGetClaimValue(AltinnAutorizationDetailsClaim, out var authDetailsJson))
        {
            return false;
        }

        var authDetailsJsonNode = JsonNode.Parse(authDetailsJson);
        if (authDetailsJsonNode is null)
        {
            return false;
        }

        // If a claim is an array, but contains only one element, it will be deserialized as a single object by dotnet
        if (authDetailsJsonNode.GetValueKind() is JsonValueKind.Array)
        {
            authorizationDetails = JsonSerializer.Deserialize<SystemUserAuthorizationDetails[]>(authDetailsJson);
        }
        else
        {
            var systemUserAuthorizationDetails = JsonSerializer.Deserialize<SystemUserAuthorizationDetails>(authDetailsJson);
            authorizationDetails = [systemUserAuthorizationDetails!];
        }

        return authorizationDetails is not null;
    }

    public static bool TryGetSystemUserId(this ClaimsPrincipal claimsPrincipal,
        [NotNullWhen(true)] out string? systemUserId)
    {
        systemUserId = null;

        if (!claimsPrincipal.TryGetAuthorizationDetailsClaimValue(out var authorizationDetails))
        {
            return false;
        }

        if (authorizationDetails.Length == 0)
        {
            return false;
        }

        var systemUserDetails = authorizationDetails.FirstOrDefault(x => x.Type == AttributeIdSystemUser);

        if (systemUserDetails?.SystemUserIds is null)
        {
            return false;
        }

        systemUserId = systemUserDetails.SystemUserIds.FirstOrDefault();

        return systemUserId is not null;
    }

    // This is used for legacy systems using Altinn 2 enterprise users with Maskinporten authentication + token exchange
    // as described in https://altinn.github.io/docs/api/rest/kom-i-gang/virksomhet/#autentisering-med-virksomhetsbruker-og-maskinporten
    public static bool TryGetLegacySystemUserId(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? systemUserId)
    {
        systemUserId = null;
        if (claimsPrincipal.TryGetClaimValue(AltinnAuthenticationMethodClaim, out var authMethod) &&
            authMethod == AltinnAuthenticationEnterpriseUserMethod &&
            claimsPrincipal.TryGetClaimValue(AltinnUserIdClaim, out var userId))
        {
            systemUserId = userId;
        }

        return systemUserId is not null;
    }

    public static bool TryGetLegacySystemUserName(this ClaimsPrincipal claimsPrincipal, [NotNullWhen(true)] out string? systemUserName)
    {
        systemUserName = null;
        if (claimsPrincipal.TryGetLegacySystemUserId(out _) &&
            claimsPrincipal.TryGetClaimValue(AltinnUserNameClaim, out var claimValue))
        {
            systemUserName = claimValue;
        }

        return systemUserName is not null;
    }

    public static bool TryGetOrganizationNumber(this Claim? consumerClaim, [NotNullWhen(true)] out string? orgNumber)
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
            c.Type == IdportenAuthLevelClaim ||
            c.Type.StartsWith(AltinnClaimPrefix, StringComparison.Ordinal)
        ).OrderBy(c => c.Type);

    public static (UserIdType, string externalId) GetUserType(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.TryGetPid(out var externalId))
        {
            // ServiceOwnerOnHelfOfPerson does not work atm., since there will be no PID claim on service owner calls
            // TODO: This needs to be fixed when implementing https://github.com/digdir/dialogporten/issues/386
            // F.ex. a middleware that runs before UserTypeValidationMiddleware that adds the PID claim
            // HERE
            // we need to add the PID claim, fetch the enduser query param and add claim to the serviceowner user
            return (claimsPrincipal.HasScope(ServiceProviderScope)
                ? UserIdType.ServiceOwnerOnBehalfOfPerson
                : UserIdType.Person, externalId);
        }

        if (claimsPrincipal.TryGetLegacySystemUserId(out externalId))
        {
            return (UserIdType.LegacySystemUser, externalId);
        }

        // https://docs.altinn.studio/authentication/systemauthentication/
        if (claimsPrincipal.TryGetSystemUserId(out externalId))
        {
            return (UserIdType.SystemUser, externalId);
        }

        if (claimsPrincipal.HasScope(ServiceProviderScope) &&
            claimsPrincipal.TryGetOrganizationNumber(out externalId))
        {
            return (UserIdType.ServiceOwner, externalId);
        }

        return (UserIdType.Unknown, string.Empty);
    }

    internal static bool TryGetOrganizationNumber(this IUser user, [NotNullWhen(true)] out string? orgNumber) =>
        user.GetPrincipal().TryGetOrganizationNumber(out orgNumber);

    internal static bool TryGetPid(this IUser user, [NotNullWhen(true)] out string? pid) =>
        user.GetPrincipal().TryGetPid(out pid);

    internal static bool TryGetLegacySystemUserId(this IUser user, [NotNullWhen(true)] out string? systemUserId) =>
        user.GetPrincipal().TryGetLegacySystemUserId(out systemUserId);

    internal static bool TryGetLegacySystemUserName(this IUser user, [NotNullWhen(true)] out string? systemUserName) =>
        user.GetPrincipal().TryGetLegacySystemUserName(out systemUserName);
}
