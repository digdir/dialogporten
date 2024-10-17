using System.Reflection;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public sealed class InfrastructureAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(InfrastructureAssemblyMarker).Assembly;
}
