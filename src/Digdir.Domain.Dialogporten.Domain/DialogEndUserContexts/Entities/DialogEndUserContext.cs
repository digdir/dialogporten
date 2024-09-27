using System.Collections.ObjectModel;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

public sealed class DialogEndUserContext : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? DialogId { get; set; }
    public DialogEntity? Dialog { get; set; }

    public SystemLabel.Values SystemLabelId { get; private set; } = SystemLabel.Values.Default;
    public SystemLabel SystemLabel { get; private set; } = null!;

    public IReadOnlyCollection<LabelAssignmentLog> LabelAssignmentLogs => _labelAssignmentLogs.AsReadOnly();
    private readonly List<LabelAssignmentLog> _labelAssignmentLogs = [];
    public void UpdateLabel(SystemLabel.Values labelId, string userId, string? userName, ActorType.Values actorType = ActorType.Values.PartyRepresentative)
    {
        if (labelId == SystemLabelId) return;
        // remove old label then add new one 
        if (SystemLabelId != SystemLabel.Values.Default)
        {
            _labelAssignmentLogs.Add(new()
            {
                Name = SystemLabelId.ToNamespacedName(),
                Action = "remove",
                PerformedBy =
                    new LabelAssignmentLogActor
                    {
                        ActorTypeId = actorType,
                        ActorId = userId,
                        ActorName = userName,
                    }
            });
        }
        if (labelId != SystemLabel.Values.Default)
        {
            _labelAssignmentLogs.Add(new()
            {
                Name = labelId.ToNamespacedName(),
                Action = "set",
                PerformedBy =
                    new LabelAssignmentLogActor
                    {
                        ActorTypeId = actorType,
                        ActorId = userId,
                        ActorName = userName,
                    }
            });
        }
        SystemLabelId = labelId;
    }

}
