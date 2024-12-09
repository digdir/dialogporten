namespace Digdir.Library.Dialogporten.WebApiClient.Interfaces;

public interface IDialogportenSettings
{
    /// <summary>
    /// ClientID to use
    /// </summary>
    string ClientId { get; set; }

    /// <summary>
    /// Scopes to request. Must be provisioned on the supplied client.
    /// </summary>
    string Scope { get; set; }

    /// <summary>
    /// Resource claim for assertion. This will be the `aud`-claim in the received access token
    /// </summary>
    string Resource { get; set; }

    /// <summary>
    /// The Maskinporten environment. Valid values are ver1, ver2, test or prod
    /// </summary>
    string Environment { get; set; }

    /// <summary>
    /// Base64 Encoded Json Web Key 
    /// </summary>
    string EncodedJwk { get; set; }
}
