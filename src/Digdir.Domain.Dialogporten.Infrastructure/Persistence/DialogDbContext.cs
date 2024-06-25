using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Common;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Content;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.ValueConverters;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence;

internal sealed class DialogDbContext : DbContext, IDialogDbContext
{
    public DialogDbContext(DbContextOptions<DialogDbContext> options) : base(options) { }

    public DbSet<DialogEntity> Dialogs => Set<DialogEntity>();
    public DbSet<DialogStatus> DialogStatuses => Set<DialogStatus>();
    public DbSet<DialogActivity> DialogActivities => Set<DialogActivity>();
    public DbSet<DialogApiAction> DialogApiActions => Set<DialogApiAction>();
    public DbSet<DialogApiActionEndpoint> DialogApiActionEndpoints => Set<DialogApiActionEndpoint>();
    public DbSet<DialogGuiAction> DialogGuiActions => Set<DialogGuiAction>();
    public DbSet<DialogElement> DialogElements => Set<DialogElement>();
    public DbSet<DialogElementUrl> DialogElementUrls => Set<DialogElementUrl>();
    public DbSet<DialogGuiActionPriority> DialogGuiActionTypes => Set<DialogGuiActionPriority>();
    public DbSet<DialogActivityType> DialogActivityTypes => Set<DialogActivityType>();
    public DbSet<DialogSeenLog> DialogSeenLog => Set<DialogSeenLog>();
    public DbSet<DialogContentType> DialogContentTypes => Set<DialogContentType>();
    public DbSet<DialogContent> DialogContent => Set<DialogContent>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers => Set<OutboxMessageConsumer>();

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
    //    optionsBuilder.LogTo(Console.WriteLine);

    internal bool TrySetOriginalRevision<TEntity>(
        TEntity? entity,
        Guid? revision)
        where TEntity : class, IVersionableEntity
    {
        if (entity is null || !revision.HasValue)
        {
            return false;
        }

        var prop = Entry(entity).Property(x => x.Revision);
        prop.OriginalValue = revision.Value;
        prop.IsModified = false;
        return true;
    }

    /// <inheritdoc/>
    public bool MustWhenModified<TEntity, TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Func<TProperty, bool> predicate)
        where TEntity : class
    {
        var property = Entry(entity).Property(propertyExpression);
        return !property.IsModified || predicate(property.CurrentValue);
    }

    public async Task<List<Guid>> GetExistingIds<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IIdentifiableEntity
    {
        var ids = entities
            .Select(x => x.Id)
            .Where(x => x != default)
            .ToList();

        return ids.Count == 0
            ? []
            : await Set<TEntity>()
                .IgnoreQueryFilters()
                .Select(x => x.Id)
                .Where(x => ids.Contains(x))
                .ToListAsync(cancellationToken);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>(x => x.HaveMaxLength(Constants.DefaultMaxStringLength));
        configurationBuilder.Properties<Uri>(x => x.HaveMaxLength(Constants.DefaultMaxUriLength));
        configurationBuilder.Properties<DateTimeOffset>().HaveConversion<DateTimeOffsetConverter>();
        configurationBuilder.Properties<TimeSpan>().HaveConversion<TimeSpanToStringConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .RemovePluralizingTableNameConvention()
            .AddAuditableEntities()
            .ApplyConfigurationsFromAssembly(typeof(DialogDbContext).Assembly);
    }
}
