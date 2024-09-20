using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

// Amund: legges til dialog create command handler
public class DialogEndUserContext : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? DialogId { get; set; }
    public DialogEntity? Dialog { get; set; }

    public SystemLabel.Values SystemLabelId { get; set; } = SystemLabel.Values.Default;
    public SystemLabel SystemLabel { get; set; } = null!;

    public List<LabelAssignmentLog> LabelAssignmentLogs { get; set; } = [];
}
