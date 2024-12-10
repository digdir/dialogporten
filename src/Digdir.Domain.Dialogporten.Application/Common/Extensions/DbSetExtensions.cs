using System.Globalization;
using System.Text;
using Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Microsoft.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions;

public static class DbSetExtensions
{
    public static IQueryable<DialogEntity> PrefilterAuthorizedDialogs(this DbSet<DialogEntity> dialogs, DialogSearchAuthorizationResult authorizedResources)
    {
        var parameters = new List<object>();
        // lang=sql
        var sb = new StringBuilder()
            .AppendLine(CultureInfo.InvariantCulture, $"""
                SELECT *
                FROM "Dialog"
                WHERE "Id" = ANY(@p{parameters.Count})
                """);
        parameters.Add(authorizedResources.DialogIds);

        // Group parties that have the same resources
        var groupedResult = authorizedResources.ResourcesByParties
            .GroupBy(kv => kv.Value, new HashSetEqualityComparer<string>())
            .ToDictionary(
                g => g.Key, // The shared HashSet<string>
                g => new HashSet<string>(g.Select(kv => kv.Key)) // Aggregate all keys with this HashSet<string>
            );

        foreach (var (resources, parties) in groupedResult)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"""
                 OR (
                    "{nameof(DialogEntity.Party)}" = ANY(@p{parameters.Count}) 
                    AND "{nameof(DialogEntity.ServiceResource)}" = ANY(@p{parameters.Count + 1})
                 )
                 """);
            parameters.Add(parties);
            parameters.Add(resources);
        }

        return dialogs.FromSqlRaw(sb.ToString(), parameters.ToArray());
    }
}

public class HashSetEqualityComparer<T> : IEqualityComparer<HashSet<T>>
{
    public bool Equals(HashSet<T>? x, HashSet<T>? y)
    {
        return ReferenceEquals(x, y) || (x is not null && y is not null && x.SetEquals(y));
    }

    public int GetHashCode(HashSet<T> obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        unchecked
        {
            return obj.Aggregate(0, (hash, item) => hash ^ (item?.GetHashCode() ?? 0));
        }
    }
}
