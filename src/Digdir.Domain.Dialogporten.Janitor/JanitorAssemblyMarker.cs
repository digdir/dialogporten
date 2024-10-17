using System.Reflection;

namespace Digdir.Domain.Dialogporten.Janitor;

public sealed class JanitorAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(JanitorAssemblyMarker).Assembly;
}
