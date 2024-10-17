using System.Reflection;

namespace Digdir.Domain.Dialogporten.Service;

public sealed class ServiceAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(ServiceAssemblyMarker).Assembly;
}
