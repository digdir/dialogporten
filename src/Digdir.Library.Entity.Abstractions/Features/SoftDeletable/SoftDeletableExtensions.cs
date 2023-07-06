namespace Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

/// <summary>
/// Provides extension methods for <see cref="ISoftDeletableEntity"/>.
/// </summary>
public static class SoftDeletableExtensions
{
    /// <summary>
    /// Marks a <see cref="ISoftDeletableEntity"/> as soft deleted.
    /// </summary>
    /// <param name="deletable">The <see cref="ISoftDeletableEntity"/> to soft delete.</param>
    /// <param name="now">The deletion time in UTC.</param>
    public static void SoftDelete(this ISoftDeletableEntity deletable, DateTimeOffset now)
    {
        deletable.DeletedAt = now;
        deletable.Deleted = true;
    }

    /// <summary>
    /// Marks a <see cref="ISoftDeletableEntity"/> as hard deleted.
    /// </summary>
    /// <remarks>
    /// This will permanently delete the entity from the database.
    /// </remarks>
    /// <param name="deletable">The <see cref="ISoftDeletableEntity"/> to permanently delete.</param>
    public static void HardDelete(this ISoftDeletableEntity deletable)
    {
        deletable.HardDelete = true;
    }

    /// <summary>
    /// Restores a <see cref="ISoftDeletableEntity"/>.
    /// </summary>
    /// <param name="deletable">The <see cref="ISoftDeletableEntity"/> to restore.</param>
    public static void Restore(this ISoftDeletableEntity deletable)
    {
        deletable.DeletedAt = null;
        deletable.Deleted = false;
    }
}
