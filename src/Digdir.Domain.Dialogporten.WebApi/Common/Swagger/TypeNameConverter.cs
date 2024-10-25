using System.Text;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Swagger;

internal static class TypeNameConverter
{
    private static readonly HashSet<string> UsedName = new();
    private static readonly string[] Prefixes =
    {
        "Digdir.Domain.Dialogporten.WebApi.Endpoints.",
        "Digdir.Domain.Dialogporten.Application.Features.",


    };
    private const string CommonNamespace = "Digdir.Domain.Dialogporten.Application.Common.";

    internal static string Convert(Type type)
    {

        var fullName = FullName(type);
        if (!UsedName.Add(fullName))
        {
            throw new InvalidOperationException($"Type {fullName} is already registered.");
        }
        return fullName;
    }

    private static string FullName(Type type)
    {
        var typeNamespace = type.Namespace!;
        var namespaceWithoutPrefix = RemovePrefix(typeNamespace);
        var collapsedNamespace = namespaceWithoutPrefix.Replace(".", "");

        var isGeneric = type.IsGenericType;
        var nameWithoutGenericArgs =
            isGeneric
                ? type.Name![..type.Name!.IndexOf('`')]
                : type.Name;

        var nameWithoutPostfix = RemovePostfix(nameWithoutGenericArgs);

        var name = isGeneric ? nameWithoutPostfix + GenericArgString(type) : nameWithoutPostfix;
        if (string.IsNullOrEmpty(collapsedNamespace))
        {
            return name;
        }
        var fullName = $"{collapsedNamespace}_{name}";
        return fullName;
    }

    private static string RemovePrefix(string typeName)
    {

        if (typeName.StartsWith(CommonNamespace, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        foreach (var prefix in Prefixes)
        {
            if (typeName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return typeName[prefix.Length..];
            }
        }
        return typeName;
    }

    private static string RemovePostfix(string typeName)
    {
        return typeName.EndsWith("Endpoint", StringComparison.Ordinal) ? typeName[..^"Endpoint".Length] : typeName;
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
