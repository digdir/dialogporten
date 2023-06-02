using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.TokenScopes;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IDialogueDbContext
{
    DbSet<DialogueEntity> Dialogues { get; }
    DbSet<Localization> Localizations { get; }
    DbSet<LocalizationSet> LocalizationSets { get; }
    DbSet<DialogueStatus> DialogueStatuses { get; }
    DbSet<DialogueTokenScope> DialogueTokenScopes { get; }
    DbSet<DialogueActivity> DialogueActivities { get; }
    DbSet<DialogueApiAction> DialogueApiActions { get; }
    DbSet<DialogueGuiAction> DialogueGuiActions { get; }
    DbSet<DialogueAttachement> DialogueAttachements { get; }
    DbSet<DialogueGuiActionType> DialogueGuiActionTypes { get; }
    DbSet<DialogueActivityType> DialogueActivityTypes { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; }
}
