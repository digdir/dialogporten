using System.Reflection;

namespace Digdir.Domain.Dialogporten.Domain;

public static class DomainAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(DomainAssemblyMarker).Assembly;
}
