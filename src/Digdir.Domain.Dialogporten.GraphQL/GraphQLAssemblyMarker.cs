using System.Reflection;

namespace Digdir.Domain.Dialogporten.GraphQL;

public static class GraphQLAssemblyMarker
{
    public static readonly Assembly Assembly = typeof(GraphQLAssemblyMarker).Assembly;
}
