using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

// Amund: legges til dialog create command handler
public sealed class DialogEndUserContext : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? DialogId { get; set; }
    public DialogEntity? Dialog { get; set; }

    public SystemLabel.Values SystemLabelId { get; set; } = SystemLabel.Values.Default;
    public SystemLabel SystemLabel { get; set; } = null!;

    public List<LabelAssignmentLog> LabelAssignmentLogs { get; set; } = [];
    public void UpdateLabel(SystemLabel.Values labelId, string userId, string? userName)
    {
        if (labelId == SystemLabelId) return;
        // remove old label then add new one 
        if (SystemLabelId != SystemLabel.Values.Default)
        {
            LabelAssignmentLogs.Add(new()
            {
                Name = SystemLabelId.ToNamespacedName(),
                Action = "remove",
                PerformedBy =
                    new LabelAssignmentLogActor
                    {
                        ActorTypeId = ActorType.Values.PartyRepresentative,
                        ActorId = userId,
                        ActorName = userName,
                    }
            });
        }
        if (labelId != SystemLabel.Values.Default)
        {
            LabelAssignmentLogs.Add(new()
            {
                Name = labelId.ToNamespacedName(),
                Action = "set",
                PerformedBy =
                    new LabelAssignmentLogActor
                    {
                        ActorTypeId = ActorType.Values.PartyRepresentative,
                        ActorId = userId,
                        ActorName = userName,
                    }
            });
        }
        SystemLabelId = labelId;
    }

}
