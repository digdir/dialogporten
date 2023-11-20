using System.Text;
using NJsonSchema.Generation;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Json;

/// <summary>
/// Use the SchemaNameGenerator from FastEndpoints as a base. Code copied from
/// https://github.com/FastEndpoints/FastEndpoints/blob/05c93fa58c013087f33176b07647e1fe07f38431/Src/Swagger/SchemaNameGenerator.cs, which is sealed :/
///
/// For this base, remove the "Dto" suffix from the generated schema names, and add a "SO" suffix to the serviceowner specific schemas. This
/// matches the "SO" suffix used on the operationIds for service owners.
///
/// </summary>
internal class SuffixedSchemaNameGenerator : ISchemaNameGenerator
{
    public string Generate(Type type)
    {
        var baseName = BaseGenerate(type);
        // TODO! Find a more typed approach
        if (type.FullName != null && (baseName.StartsWith("GetDialog") || baseName.StartsWith("ListDialog") || baseName.StartsWith("PaginatedList")) && type.FullName.Contains(".ServiceOwner"))
        {
            baseName += "SO";
        }
        return baseName;
    }

    private string BaseGenerate(Type type)
    {
        var isGeneric = type.IsGenericType;
        var fullNameWithoutGenericArgs =
            isGeneric
                ? type.FullName![..type.FullName!.IndexOf('`')]
                : type.FullName;

        var index = fullNameWithoutGenericArgs!.LastIndexOf('.');
        index = index == -1 ? 0 : index + 1;
        var shortName = fullNameWithoutGenericArgs[index..];

        return isGeneric
            ? shortName + GenericArgString(type)
            : shortName;

        static string GenericArgString(Type type)
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
                sb.Append(GenericArgString(arg));
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
}
