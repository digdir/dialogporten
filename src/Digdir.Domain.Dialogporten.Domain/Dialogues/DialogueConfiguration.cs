namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueConfiguration
{
    public List<string> ServiceProviderScopesRequired { get; set; } = new();
    public DateTime VisibleFrom { get; set; }
}
