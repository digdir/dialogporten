using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Parties;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Common;

public interface IDialogTokenGenerator
{
    string GetDialogToken(DialogEntity dialog, DialogDetailsAuthorizationResult authorizationResult);
}

internal class DialogTokenGenerator : IDialogTokenGenerator
{
    private readonly ApplicationSettings _applicationSettings;
    private readonly IUser _user;
    private readonly ICompactJwsGenerator _compactJwsGenerator;

    // 30 minutes of idling will presumably cause a session timeout anyway,
    // so having the token being invalid after 30 minutes should be sufficient.
    private readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(30);

    public DialogTokenGenerator(
        IOptions<ApplicationSettings> applicationSettings,
        IUser user,
        ICompactJwsGenerator compactJwsGenerator)
    {
        _applicationSettings = applicationSettings.Value ?? throw new ArgumentNullException(nameof(applicationSettings));
        _user = user ?? throw new ArgumentNullException(nameof(user));
        _compactJwsGenerator = compactJwsGenerator ?? throw new ArgumentNullException(nameof(compactJwsGenerator));
    }

    public string GetDialogToken(DialogEntity dialog, DialogDetailsAuthorizationResult authorizationResult)
    {
        var claimsPrincipal = _user.GetPrincipal();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var dt = new DialogTokenClaims
        {
            JwtId = Guid.NewGuid(),
            DialogId = dialog.Id,
            AuthenticatedParty = GetAuthenticatedParty(),
            AuthenticationLevel = claimsPrincipal.TryGetAuthenticationLevel(out var authenticationLevel)
                ? authenticationLevel.Value
                : 0,
            DialogParty = dialog.Party,
            SupplierParty = claimsPrincipal.TryGetSupplierOrgNumber(out var supplierOrgNumber)
                ? NorwegianOrganizationIdentifier.PrefixWithSeparator + supplierOrgNumber
                : null,
            Actions = GetAuthorizedActions(authorizationResult),
            Issuer = _applicationSettings.Dialogporten.BaseUri.ToString(),
            IssuedAt = now,
            NotBefore = now,
            Expires = now + (long)_tokenLifetime.TotalSeconds
        };

        return _compactJwsGenerator.GetCompactJws(dt);
    }

    private static string GetAuthorizedActions(DialogDetailsAuthorizationResult authorizationResult)
    {
        var actions = new StringBuilder();
        foreach (var (action, resource) in authorizationResult.AuthorizedAltinnActions)
        {
            actions.Append(action);
            if (resource != Authorization.Constants.MainResource)
            {
                actions.Append(CultureInfo.InvariantCulture, $",{resource}");
            }

            actions.Append(';');
        }

        // Remove trailing semicolon
        actions.Remove(actions.Length - 1, 1);

        return actions.ToString();
    }

    private string GetAuthenticatedParty()
    {
        if (_user.TryGetPid(out var pid))
        {
            return NorwegianPersonIdentifier.PrefixWithSeparator + pid;
        }

        if (_user.TryGetOrgNumber(out var orgNumber))
        {
            return NorwegianOrganizationIdentifier.PrefixWithSeparator + orgNumber;
        }

        throw new InvalidOperationException("User must have either pid or org number");
    }
}

// TODO! Consider moving this to the domain layer
public sealed class DialogTokenClaims
{
    [JsonPropertyName("l")]
    public int AuthenticationLevel { get; set; }

    [JsonPropertyName("c")]
    public string AuthenticatedParty { get; set; } = null!;

    [JsonPropertyName("p")]
    public string DialogParty { get; set; } = null!;

    [JsonPropertyName("s")]
    public string? SupplierParty { get; set; }

    [JsonPropertyName("i")]
    public Guid DialogId { get; set; }

    [JsonPropertyName("a")]
    public string Actions { get; set; } = null!;

    [JsonPropertyName("iss")]
    public string Issuer { get; set; } = null!;

    [JsonPropertyName("exp")]
    public long Expires { get; set; }

    [JsonPropertyName("nbf")]
    public long NotBefore { get; set; }

    [JsonPropertyName("iat")]
    public long IssuedAt { get; set; }

    [JsonPropertyName("jti")]
    public Guid JwtId { get; set; }
}
