using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Digdir.Library.Entity.Abstractions.Features.Lookup;

/// <inheritdoc cref="ILookupEntity{TSelf, TEnum}"/>
public abstract class AbstractLookupEntity<TSelf, TEnum> : ILookupEntity<TSelf, TEnum>
    where TSelf : AbstractLookupEntity<TSelf, TEnum>
    where TEnum : Enum
{
    /// <inheritdoc/>
    public TEnum Id { get; private set; }

    /// <inheritdoc/>
    public string Name { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractLookupEntity{TSelf, TEnum}"/> class.
    /// </summary>
    /// <remarks>
    /// <see cref="Name"/> will be populated using ToString() on <paramref name="id"/>.
    /// </remarks>
    /// <param name="id">The entity identification.</param>
    protected AbstractLookupEntity(TEnum id) : this(id, id.ToString()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractLookupEntity{TSelf, TEnum}"/> class.
    /// </summary>
    /// <param name="id">The entity identification.</param>
    /// <param name="name">A human readable name to identify the lookup type.</param>
    protected AbstractLookupEntity(TEnum id, string name)
    {
        if (!Enum.IsDefined(typeof(TEnum), id))
        {
            throw new InvalidEnumArgumentException(nameof(id), (int)(object)id, typeof(TEnum));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        Id = id;
        Name = name;
    }

    /// <summary>
    /// Maps to <typeparamref name="TSelf"/> from <typeparamref name="TEnum"/>.
    /// </summary>
    /// <param name="id">The entity identification.</param>
    /// <returns>A new instance of <typeparamref name="TSelf"/>.</returns>
    public abstract TSelf MapValue(TEnum id);

    /// <summary>
    /// Gets a new instance of <typeparamref name="TSelf"/> corresponding to the incoming <typeparamref name="TEnum"/>.
    /// </summary>
    /// <param name="id">The entity identification.</param>
    /// <returns>A new instance of <typeparamref name="TSelf"/>.</returns>
    public static TSelf GetValue(TEnum id) => (TSelf)id;

    /// <summary>
    /// Gets all the values of <typeparamref name="TSelf"/>.
    /// </summary>
    /// <returns>An enumerable containing all the possible values of <typeparamref name="TSelf"/>.</returns>
    public static IEnumerable<TSelf> GetValues()
    {
        var self = GetUninitializedSelf();
        var enums = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
        foreach (var @enum in enums)
        {
            yield return self.MapValue(@enum);
        }
    }

    /// <inheritdoc/>
    public static bool operator ==(AbstractLookupEntity<TSelf, TEnum> left, AbstractLookupEntity<TSelf, TEnum> right)
    {
        if (left is null ^ right is null)
        {
            return false;
        }

        return left?.Equals(right) != false;
    }

    /// <inheritdoc/>
    public static bool operator !=(AbstractLookupEntity<TSelf, TEnum> left, AbstractLookupEntity<TSelf, TEnum> right)
    {
        return !(left == right);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (AbstractLookupEntity<TSelf, TEnum>)obj;
        return Id.Equals(other.Id);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Id.GetHashCode();

    /// <inheritdoc/>
    public static explicit operator TEnum(AbstractLookupEntity<TSelf, TEnum> entity)
    {
        return entity.Id;
    }

    /// <inheritdoc/>
    public static explicit operator AbstractLookupEntity<TSelf, TEnum>(TEnum id)
    {
        return GetUninitializedSelf().MapValue(id);
    }

    private static TSelf GetUninitializedSelf() => (TSelf)RuntimeHelpers.GetUninitializedObject(typeof(TSelf));
}
