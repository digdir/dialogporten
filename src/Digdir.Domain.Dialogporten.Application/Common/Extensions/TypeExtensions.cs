namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

internal static class TypeExtensions
{
    public static bool IsNullableType(this Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
}
