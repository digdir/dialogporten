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
    /// <param name="utcNow">The creation time in UTC.</param>
    public static void Create(this ICreatableEntity creatable, DateTimeOffset utcNow)
    {
        creatable.CreatedAt = creatable.CreatedAt == default 
            ? utcNow 
            : creatable.CreatedAt;
    }
}
