using System.Collections.ObjectModel;
using Digdir.Domain.Dialogporten.Domain.Actors;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.DialogEndUserContexts.Entities;

public sealed class DialogEndUserContext : IEntity
{
    private readonly List<LabelAssignmentLog> _labelAssignmentLogs = [];

    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Guid? DialogId { get; set; }
    public DialogEntity? Dialog { get; set; }

    public SystemLabel.Values SystemLabelId { get; private set; } = SystemLabel.Values.Default;
    public SystemLabel SystemLabel { get; private set; } = null!;

    public IReadOnlyCollection<LabelAssignmentLog> LabelAssignmentLogs => _labelAssignmentLogs.AsReadOnly();

    public void UpdateLabel(SystemLabel.Values newLabel, string userId, string? userName, ActorType.Values actorType = ActorType.Values.PartyRepresentative)
    {
        var currentLabel = SystemLabelId;
        if (newLabel == currentLabel)
        {
            return;
        }

        // remove old label then add new one 
        if (currentLabel != SystemLabel.Values.Default)
        {
            _labelAssignmentLogs.Add(new()
            {
                Name = currentLabel.ToNamespacedName(),
                Action = "remove",
                PerformedBy = new()
                {
                    ActorTypeId = actorType,
                    ActorId = userId,
                    ActorName = userName,
                }
            });
        }

        if (newLabel != SystemLabel.Values.Default)
        {
            _labelAssignmentLogs.Add(new()
            {
                Name = newLabel.ToNamespacedName(),
                Action = "set",
                PerformedBy = new()
                {
                    ActorTypeId = actorType,
                    ActorId = userId,
                    ActorName = userName,
                }
            });
        }

        SystemLabelId = newLabel;
    }
}
