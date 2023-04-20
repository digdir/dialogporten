namespace Digdir.Library.Entity.Abstractions.Features.Creatable;

/// <summary>
/// Provides extension methods for <see cref="ICreatableEntity"/>.
/// </summary>
public static class CreatableExtensions
{
    /// <summary>
    /// Sets properties related to <see cref="ICreatableEntity"/>.
    /// </summary>
    /// <param name="creatable">The <see cref="ICreatableEntity"/> to update.</param>
    /// <param name="userId">The id of the user that created this entity.</param>
    /// <param name="utcNow">The creation time in UTC.</param>
    public static void Create(this ICreatableEntity creatable, Guid userId, DateTimeOffset utcNow)
    {
        creatable.CreatedAtUtc = creatable.CreatedAtUtc == default 
            ? utcNow 
            : creatable.CreatedAtUtc;
        creatable.CreatedByUserId = userId;
    }
}
