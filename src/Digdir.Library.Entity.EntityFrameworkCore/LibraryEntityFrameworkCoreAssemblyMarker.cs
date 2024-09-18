using System.Reflection;

namespace Digdir.Library.Entity.EntityFrameworkCore;

/// <summary>
/// Provides a marker for the Entity Framework Core assembly.
/// </summary>
public static class LibraryEntityFrameworkCoreAssemblyMarker
{
    /// <summary>
    /// Gets the assembly of the Entity Framework Core.
    /// </summary>
    public static readonly Assembly Assembly = typeof(LibraryEntityFrameworkCoreAssemblyMarker).Assembly;
}
