using System.Reflection;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public static class InfrastructureAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(InfrastructureAssemblyMarker).Assembly;
}
