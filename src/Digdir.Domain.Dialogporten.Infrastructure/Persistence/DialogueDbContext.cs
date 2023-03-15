using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogues.Entities.TokenScopes;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

internal sealed class DialogueDbContext : DbContext, IDialogueDbContext
{
    public DialogueDbContext(DbContextOptions<DialogueDbContext> options) : base(options) { }

    public DbSet<DialogueEntity> Dialogues => Set<DialogueEntity>();
    public DbSet<Localization> Localizations => Set<Localization>();
    public DbSet<LocalizationSet> LocalizationSets => Set<LocalizationSet>();
    public DbSet<DialogueStatus> DialogueStatuses => Set<DialogueStatus>();
    public DbSet<DialogueTokenScope> DialogueTokenScopes => Set<DialogueTokenScope>();
    public DbSet<DialogueActivity> DialogueActivities => Set<DialogueActivity>();
    public DbSet<DialogueApiAction> DialogueApiActions => Set<DialogueApiAction>();
    public DbSet<DialogueGuiAction> DialogueGuiActions => Set<DialogueGuiAction>();
    public DbSet<DialogueAttachement> DialogueAttachements => Set<DialogueAttachement>();
    public DbSet<DialogueGuiActionType> DialogueGuiActionTypes => Set<DialogueGuiActionType>();
    public DbSet<DialogueActivityType> DialogueActivityTypes => Set<DialogueActivityType>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>(x => x.HaveMaxLength(255));
        configurationBuilder.Properties<Uri>(x => x.HaveMaxLength(1023));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.RemovePluralizingTableNameConvention()
            .SetCrossCuttingTimeSpanToStringConverter()
            .AddAuditableEntities()
            .ApplyConfigurationsFromAssembly(typeof(DialogueDbContext).Assembly);
}