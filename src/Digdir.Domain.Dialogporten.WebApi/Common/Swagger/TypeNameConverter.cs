using System.Net.Http.Headers;
using System.Text;
using MessagePack.Resolvers;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Swagger;

internal static class TypeNameConverter
{
    // Amund: Bruk av dict gjør feilmeldingene mer tydelig på hva som er galt.
    // private static readonly HashSet<string> RegisteredNames = new();
    private static readonly Dictionary<string, string> RegisteredNamesMap = new();

    private static readonly string[] RemovedNamespace =
    {
        "Digdir.Domain.Dialogporten.WebApi.Endpoints.",
        "Digdir.Domain.Dialogporten.Application.Features."
    };

    // Common namespace has unique handling, Can't be added to list above
    private const string CommonNamespace = "Digdir.Domain.Dialogporten.Application.Common.";
    private const string FastEndpointsNamespace = "FastEndpoints";

    private const string EndpointPostfix = "Endpoint";

    public static string Convert(Type type)
    {
        var fullName = FullName(type);
        // if (!RegisteredNames.Add(fullName))
        // {
        //     throw new ArgumentException($"{type.FullName} can't be registered. Type {fullName} is already registered.");
        // }

        if (RegisteredNamesMap.TryGetValue(fullName, out var value))
        {
            throw new ArgumentException($"{type.FullName} can't be registered. Type {fullName} is already registered by {value}.");
        }
        RegisteredNamesMap.Add(fullName, type.FullName!);

        return fullName;
    }

    private static string FullName(Type type)
    {
        var isGeneric = type.IsGenericType;

        // When parsing generics we can get a type without namespace.
        // FullName is needed for nested classes.
        string shortName;
        if (type.FullName is not null)
        {
            var nameWithoutGenericArgs = isGeneric
                ? type.FullName![..type.FullName!.IndexOf('`')]
                : type.FullName;
            var index = nameWithoutGenericArgs!.LastIndexOf('.');
            index = index == -1 ? 0 : index + 1;
            shortName = nameWithoutGenericArgs[index..];
        }
        else
        {
            shortName = isGeneric
                ? type.Name[..type.Name.IndexOf('`')]
                : type.Name;
        }

        var nameWithoutPostfix =
            shortName.EndsWith(EndpointPostfix, StringComparison.Ordinal)
                ? shortName[..^EndpointPostfix.Length]
                : shortName;

        var name = isGeneric ? nameWithoutPostfix + GenericArgString(type) : nameWithoutPostfix;

        if (type.Namespace!.StartsWith(CommonNamespace, StringComparison.Ordinal) || type.Namespace.StartsWith(FastEndpointsNamespace, StringComparison.Ordinal))
        {
            return name;
        }

        var collapsedNamespace = RemoveNamespace(type.Namespace!).Replace(".", "");

        return $"{collapsedNamespace}_{name}";
    }

    private static string RemoveNamespace(string typeName)
    {
        foreach (var prefix in RemovedNamespace)
        {
            if (typeName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return typeName[prefix.Length..];
            }
        }
        return typeName;
    }

    private static string GenericArgString(Type type)
    {
        if (!type.IsGenericType) return type.Name;

        var sb = new StringBuilder();
        var args = type.GetGenericArguments();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (i == 0)
                sb.Append("Of");
            sb.Append(TypeNameWithoutGenericArgs(arg));
            // Amund: problemet oppstår her, type som blir sendt in har ikke FullName og ingen Namespace og derfor funker den ikke
            sb.Append(FullName(arg));
            if (i < args.Length - 1)
                sb.Append("And");
        }

        return sb.ToString();

        static string TypeNameWithoutGenericArgs(Type type)
        {
            var index = type.Name.IndexOf('`');
            index = index == -1 ? 0 : index;

            return type.Name[..index];
        }
    }
}
