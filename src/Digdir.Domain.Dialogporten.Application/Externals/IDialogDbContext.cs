using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Elements;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IDialogDbContext
{
    DbSet<DialogEntity> Dialogs { get; }
    DbSet<DialogStatus> DialogStatuses { get; }
    DbSet<DialogActivity> DialogActivities { get; }
    DbSet<DialogApiAction> DialogApiActions { get; }
    DbSet<DialogApiActionEndpoint> DialogApiActionEndpoints { get; }
    DbSet<DialogGuiAction> DialogGuiActions { get; }
    DbSet<DialogElement> DialogElements { get; }
    DbSet<DialogElementUrl> DialogElementUrls { get; }
    DbSet<DialogGuiActionPriority> DialogGuiActionTypes { get; }
    DbSet<DialogActivityType> DialogActivityTypes { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; }
    DbSet<DialogSeenLog> DialogSeenLog { get; }

    /// <summary>
    /// Validate a property on the <typeparamref name="TEntity"/> using a lambda 
    /// expression to specify the predicate only when the property is modified.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    /// <param name="entity"></param>
    /// <param name="propertyExpression"></param>
    /// <param name="predicate"></param>
    /// <returns>
    ///     <para>False if the property is modified and the predicate returns false.</para>
    ///     <para>True if the property is unmodified or the predicate returns true.</para>
    /// </returns>
    bool MustWhenModified<TEntity, TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression,
        Func<TProperty, bool> predicate)
        where TEntity : class;
    Task<List<Guid>> GetExistingIds<TEntity>(
        IEnumerable<TEntity> entities,
        CancellationToken cancellationToken)
        where TEntity : class, IIdentifiableEntity;
}
