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
    /// <param name="userId">The id of the user that deleted this entity.</param>
    /// <param name="now">The deletion time in UTC.</param>
    public static void Delete(this ISoftDeletableEntity deletable, Guid userId, DateTimeOffset now)
    {
        deletable.DeletedByUserId = userId;
        deletable.DeletedAtUtc = now;
        deletable.Deleted = true;
    }

    /// <summary>
    /// Restores a <see cref="ISoftDeletableEntity"/>.
    /// </summary>
    /// <param name="deletable">The <see cref="ISoftDeletableEntity"/> to restore.</param>
    public static void Restore(this ISoftDeletableEntity deletable)
    {
        deletable.DeletedByUserId = null;
        deletable.DeletedAtUtc = null;
        deletable.Deleted = false;
    }
}
