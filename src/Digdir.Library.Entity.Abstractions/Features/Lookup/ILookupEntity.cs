namespace Digdir.Library.Entity.Abstractions.Features.Lookup;

/// <inheritdoc cref="ILookupEntity"/>
/// <typeparam name="TSelf">
/// The concrete implementing type.
/// </typeparam>
/// <typeparam name="TEnum">
/// The <see cref="Enum"/> values that represents the full
/// set of possible static entity values. Each
/// <see cref="Enum"/> value is a unique id for the
/// <see cref="ILookupEntity"/>.
/// </typeparam>
public interface ILookupEntity<TSelf, TEnum> : ILookupEntity
    where TSelf : ILookupEntity<TSelf, TEnum>
    where TEnum : Enum
{
    /// <summary>
    /// The entity identification.
    /// </summary>
    TEnum Id { get; }
}

/// <summary>
/// Abstraction implemented by entities that is static during the application
/// lifetime. For example, type information, or states in a finite state machine.
/// </summary>
public interface ILookupEntity
{
    /// <summary>
    /// A human-readable name to identify the lookup type.
    /// </summary>
    string Name { get; }
}
