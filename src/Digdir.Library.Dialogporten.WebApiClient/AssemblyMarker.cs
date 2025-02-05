using System.Reflection;

namespace Digdir.Library.Dialogporten.WebApiClient;

internal sealed class AssemblyMarker
{
    
    public static readonly Assembly Assembly = typeof(AssemblyMarker).Assembly;
}
