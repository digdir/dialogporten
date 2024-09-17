using System.Reflection;

namespace Digdir.Domain.Dialogporten.Janitor;

public static class JanitorAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(JanitorAssemblyMarker).Assembly;
}
