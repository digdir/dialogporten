using System.Reflection;

namespace Digdir.Domain.Dialogporten.GraphQL;

public sealed class GraphQLAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(GraphQLAssemblyMarker).Assembly;
}
