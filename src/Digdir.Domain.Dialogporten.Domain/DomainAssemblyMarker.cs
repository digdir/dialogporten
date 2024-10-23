using System.Reflection;

namespace Digdir.Domain.Dialogporten.Domain;

public sealed class DomainAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(DomainAssemblyMarker).Assembly;
}
