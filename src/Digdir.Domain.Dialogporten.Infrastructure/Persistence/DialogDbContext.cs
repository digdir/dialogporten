using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.DialogElements;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.Configurations.Localizations;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.ValueConverters;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

internal sealed class DialogDbContext : DbContext, IDialogDbContext
{
    public DialogDbContext(DbContextOptions<DialogDbContext> options) : base(options) { }
    private DialogDbContext() { }

    public DbSet<DialogEntity> Dialogs => Set<DialogEntity>();
    public DbSet<Localization> Localizations => Set<Localization>();
    public DbSet<LocalizationSet> LocalizationSets => Set<LocalizationSet>();
    public DbSet<DialogStatus> DialogStatuses => Set<DialogStatus>();
    public DbSet<DialogActivity> DialogActivities => Set<DialogActivity>();
    public DbSet<DialogApiAction> DialogApiActions => Set<DialogApiAction>();
    public DbSet<DialogApiActionEndpoint> DialogApiActionEndpoints => Set<DialogApiActionEndpoint>();
    public DbSet<DialogGuiAction> DialogGuiActions => Set<DialogGuiAction>();
    public DbSet<DialogElement> DialogElements => Set<DialogElement>();
    public DbSet<DialogElementUrl> DialogElementUrls => Set<DialogElementUrl>();
    public DbSet<DialogGuiActionPriority> DialogGuiActionTypes => Set<DialogGuiActionPriority>();
    public DbSet<DialogActivityType> DialogActivityTypes => Set<DialogActivityType>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers => Set<OutboxMessageConsumer>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>(x => x.HaveMaxLength(255));
        configurationBuilder.Properties<Uri>(x => x.HaveMaxLength(1023));
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.RemovePluralizingTableNameConvention()
            .SetCrossCuttingTimeSpanToStringConverter()
            .AddAuditableEntities()
            .ApplyLocalizationSetRestrictDeleteBehaviour()
            .ApplyConfigurationsFromAssembly(typeof(DialogDbContext).Assembly);
    }
}
