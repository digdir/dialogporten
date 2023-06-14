namespace Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

/// <summary>
/// Provides extension methods for <see cref="ISoftDeletableEntity"/>.
/// </summary>
public static class SoftDeletableExtensions
{
    /// <summary>
    /// Marks a <see cref="ISoftDeletableEntity"/> as deleted.
    /// </summary>
    /// <param name="deletable">The <see cref="ISoftDeletableEntity"/> to delete.</param>
    /// <param name="now">The deletion time in UTC.</param>
    public static void Delete(this ISoftDeletableEntity deletable, DateTimeOffset now)
    {
        deletable.DeletedAt = now;
        deletable.Deleted = true;
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
