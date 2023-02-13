using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueAttachements
{
    public LocalizationSet DisplayName { get; set; } = null!;
    public long SizeInBytes { get; set; }
    public string ContentType { get; set; } = null!;
    public Uri Url { get; set; } = null!;

    /// <summary>
    /// Det kan oppgis en valgfri referanse til en ressurs. Brukeren må ha tilgang til "open" i
    /// XACML-policy for oppgitt ressurs for å få tilgang til dialogen.
    /// </summary>
    public string? Resource { get; set; }
}
