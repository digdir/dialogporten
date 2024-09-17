using System.Reflection;

namespace Digdir.Library.Entity.Abstractions;

/// <summary>
/// Provides a marker for the Entity Abstractions assembly.
/// </summary>
public static class LibraryEntityAbstractionsAssemblyMarker
{
    /// <summary>
    /// Gets the assembly of the Entity Abstractions.
    /// </summary>
    public static readonly Assembly Assembly = typeof(LibraryEntityAbstractionsAssemblyMarker).Assembly;
}
