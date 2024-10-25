using System.Text;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.Migrations;

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

        var nameWithoutGenericArgs =
            isGeneric
                ? type.Name[..type.Name.IndexOf('`')]
                : type.Name;

        var nameWithoutPostfix =
            nameWithoutGenericArgs.EndsWith(EndpointPostfix, StringComparison.Ordinal)
                ? nameWithoutGenericArgs[..^EndpointPostfix.Length]
                : nameWithoutGenericArgs;

        var name = isGeneric ? nameWithoutPostfix + GenericArgString(type) : nameWithoutPostfix;

        if (type.Namespace!.StartsWith(CommonNamespace, StringComparison.Ordinal))
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
