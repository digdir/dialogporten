using Altinn.ApiClients.Maskinporten.Config;

namespace Altinn.ApiClients.Dialogporten.Config;

public sealed class DialogportenSettings
{
    public string BaseUri { get; set; } = null!;
    public MaskinportenSettings Maskinporten { get; set; } = null!;
#pragma warning disable CA1822 // Mark members as static
    internal bool Validate() => true;
#pragma warning restore CA1822 // Mark members as static
}
