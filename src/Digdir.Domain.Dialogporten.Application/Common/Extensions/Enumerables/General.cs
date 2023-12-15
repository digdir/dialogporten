using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.Enumerables;

internal static class General
{
    internal static bool IsNullOrEmpty([NotNullWhen(false)] this ICollection? values) => values is null || values.Count == 0;
}
