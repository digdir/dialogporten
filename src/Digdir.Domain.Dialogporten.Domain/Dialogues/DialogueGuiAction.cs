using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueGuiAction
{
    public string Action { get; set; } = null!;
    public DialogueGuiActionType Type { get; set; } = null!;
    public LocalizationSet Title { get; set; } = null!;
    public Uri Url { get; set; } = null!;
    public string? Resource { get; set; }
    public bool IsBackChannel { get; set; }
    public bool IsDeleteAction { get; set; }
}
