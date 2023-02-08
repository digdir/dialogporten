using Digdir.Library.Entity.EntityFrameworkCore.Features.Creatable;
using Digdir.Library.Entity.EntityFrameworkCore.Features.Identifiable;
using Digdir.Library.Entity.EntityFrameworkCore.Features.Lookup;
using Digdir.Library.Entity.EntityFrameworkCore.Features.SoftDeletable;
using Digdir.Library.Entity.EntityFrameworkCore.Features.Updatable;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Digdir.Library.Entity.Abstractions.Features.Creatable;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using Digdir.Library.Entity.Abstractions.Features.Lookup;
using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;
using Digdir.Library.Entity.Abstractions.Features.Updatable;

namespace Digdir.Library.Entity.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for EntityFrameworkCore.
/// </summary>
public static class EntityLibraryEfCoreExtensions
{
    /// <summary>
    /// Updates the properties and sets the correct <see cref="EntityState"/> on the <see cref="ChangeTracker"/> for the entities implementing the following abstractions.
    /// <list type="bullet">
    ///     <item><see cref="ILookupEntity"/></item>
    ///     <item><see cref="IIdentifiableEntity"/></item>
    ///     <item><see cref="ICreatableEntity"/></item>
    ///     <item><see cref="IUpdateableEntity"/></item>
    ///     <item><see cref="ISoftDeletableEntity"/></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Should be called right before saving the entities.
    /// </remarks>
    /// <param name="changeTracker">The change tracker.</param>
    /// <param name="userId">The id of the user that made the current changes.</param>
    /// <param name="utcNow">The time in UTC in which the changes tok place.</param>
    /// <returns>The same <see cref="ChangeTracker"/> instance so that multiple calls can be chained.</returns>
    public static ChangeTracker HandleAuditableEntities(this ChangeTracker changeTracker, Guid userId, DateTime utcNow)
    {
        return changeTracker
            .HandleLookupEntities()
            .HandleIdentifiableEntities()
            .HandleCreatableEntities(userId, utcNow)
            .HandleUpdatableEntities(userId, utcNow)
            .HandleSoftDeletableEntities(userId, utcNow);
    }

    /// <summary>
    /// Configures the shape of, and how the entities implementing the following abstractions are maped to the database.
    /// <list type="bullet">
    ///     <item><see cref="ILookupEntity"/></item>
    ///     <item><see cref="IIdentifiableEntity"/></item>
    ///     <item><see cref="ICreatableEntity"/></item>
    ///     <item><see cref="IUpdateableEntity"/></item>
    ///     <item><see cref="ISoftDeletableEntity"/></item>
    /// </list>
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder AddAuditableEntities(this ModelBuilder modelBuilder)
    {
        return modelBuilder
            .EnableSoftDeletableQueryFilter()
            .AddIdentifiableEntities()
            .AddUpdatableEntities()
            .AddCreatableEntities()
            .AddLookupEntities();
    }

    /// <summary>
    /// Removes pluralization in table names despite having pluralizing names on the contexts <see cref="DbSet{TEntity}"/> properties.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.DisplayName());
        }

        return modelBuilder;
    }

    /// <summary>
    /// Sets the <see cref="TimeSpanToStringConverter"/> for all <see cref="TimeSpan"/> properties of all the entities in the <see cref="DbContext"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    public static ModelBuilder SetCrossCuttingTimeSpanToStringConverter(this ModelBuilder modelBuilder)
    {
        var timeSpanTypes = new[] { typeof(TimeSpan), typeof(TimeSpan?) };

        var stringProperties = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(x => x.GetProperties())
            .Where(x => timeSpanTypes.Contains(x.ClrType));

        foreach (var stringProperty in stringProperties)
        {
            stringProperty.SetValueConverter(new TimeSpanToStringConverter());
        }

        return modelBuilder;
    }

    /// <summary>
    /// Removes the maximum length restriction of data that is allowed in this property.
    /// </summary>
    /// <param name="builder">The property builder.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public static PropertyBuilder<string> HasUnlimitedLength(this PropertyBuilder<string> builder)
    {
        builder.Metadata.SetMaxLength(null);
        return builder;
    }

    internal static ModelBuilder EntitiesOfType<TEntity>(this ModelBuilder modelBuilder, Action<EntityTypeBuilder> buildAction) where TEntity : class
    {
        return modelBuilder.EntitiesOfType(typeof(TEntity), buildAction);
    }

    internal static ModelBuilder EntitiesOfType(this ModelBuilder modelBuilder, Type type,
        Action<EntityTypeBuilder> buildAction)
    {
        var entities = modelBuilder.Model
            .GetEntityTypes()
            .Where(x => type.IsAssignableFrom(x.ClrType));

        foreach (var entity in entities)
        {
            buildAction(modelBuilder.Entity(entity.ClrType));
        }

        return modelBuilder;
    }
}
