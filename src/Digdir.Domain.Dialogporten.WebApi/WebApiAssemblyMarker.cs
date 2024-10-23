using System.Reflection;

namespace Digdir.Domain.Dialogporten.WebApi;

public sealed class WebApiAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(WebApiAssemblyMarker).Assembly;
}
