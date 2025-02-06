using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.ApiClients.Dialogporten.Config;

public sealed class DialogportenSettings
{
    public string BaseUri { get; set; } = null!;
    public MaskinportenSettings Maskinporten { get; set; } = null!;
    public Ed25519Keys Ed25519Keys { get; set; } = null!;
#pragma warning disable CA1822 // Mark members as static
    internal bool Validate()
    {
        return true;
    }
#pragma warning restore CA1822 // Mark members as static
}

public sealed record Ed25519Keys
{
    public Ed25519Key Primary { get; set; } = null!;
    public Ed25519Key Secondary { get; set; } = null!;
}

public sealed record Ed25519Key
{
    public string Kid { get; set; } = null!;
    public string PublicComponent { get; set; } = null!;
}
