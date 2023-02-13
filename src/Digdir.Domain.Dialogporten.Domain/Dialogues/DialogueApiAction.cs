namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueApiAction
{
    public string Action { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    // TODO: Skal vi ha noe strengere validering her?
    public string HttpMethod { get; set; } = null!;
    public string? Resource { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }

    public Uri? DocumentationUrl { get; set; }
    public Uri? RequestSchema { get; set; }
    // Hvorfor trenger vi dette?
    public Uri? ResponseSchema { get; set; }
}
