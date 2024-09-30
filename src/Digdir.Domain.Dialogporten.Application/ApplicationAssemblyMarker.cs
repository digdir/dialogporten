using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application;

public static class ApplicationAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(ApplicationAssemblyMarker).Assembly;
}
