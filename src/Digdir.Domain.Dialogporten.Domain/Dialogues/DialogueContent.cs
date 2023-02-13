using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Domain.Dialogues;

public class DialogueContent
{
    public LocalizationSet Body { get; set; } = null!;
    public LocalizationSet Title { get; set; } = null!;
    public LocalizationSet SenderName { get; set; } = null!;
}
