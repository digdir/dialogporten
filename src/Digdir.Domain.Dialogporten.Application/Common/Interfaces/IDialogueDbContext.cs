using Digdir.Domain.Dialogporten.Domain.Dialogues;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Attachments;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Interfaces;

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
}
