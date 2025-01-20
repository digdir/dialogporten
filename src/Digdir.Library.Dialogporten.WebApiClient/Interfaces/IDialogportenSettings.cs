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
    MaskinportenEnvironment Environment { get; set; }

    /// <summary>
    /// Base64 Encoded Json Web Key 
    /// </summary>
    /// <remarks>
    /// Security considerations:
    /// - Store this value in a secure configuration store or key vault
    /// - Ensure the value is properly Base64 encoded
    /// - Protect this value in memory and logs
    /// </remarks>
    string EncodedJwk { get; set; }
}

public enum MaskinportenEnvironment
{
    /// <summary>
    /// Version 1 environment
    /// </summary>
    Ver1,

    /// <summary>
    /// Version 2 environment
    /// </summary>
    Ver2,

    /// <summary>
    /// Test environment
    /// </summary>
    Test,

    /// <summary>
    /// Production environment
    /// </summary>
    Prod

}
