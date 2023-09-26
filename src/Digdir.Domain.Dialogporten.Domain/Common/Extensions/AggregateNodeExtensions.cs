using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Domain.Common.Extensions;

public static class AggregateNodeExtensions
{
    public static IReadOnlyCollection<string> ToPaths(this IEnumerable<AggregateNode> aggregateNodes) =>
        ToPaths(aggregateNodes, string.Empty);
    private static IReadOnlyCollection<string> ToPaths(this IEnumerable<AggregateNode> aggregateNodes, string parentPath)
    {
        var paths = new List<string>();

        foreach (var node in aggregateNodes)
        {
            if (node.Entity is LocalizationSet localizationSet)
            {
                paths.AddRange(localizationSet.ToLocalizationPathStrings(
                    $"{parentPath}/{node.Entity.GetType().Name}"));
                continue;
            }

            if (node.Entity is not IIdentifiableEntity identifiable)
            {
                continue;
            }

            var currentPath = $"{parentPath}/{node.Entity.GetType().Name}/{identifiable.Id}";
            paths.AddRange(node.Children.ToPaths(currentPath).DefaultIfEmpty(currentPath));
        }

        return paths;
    }

    private static IEnumerable<string> ToLocalizationPathStrings(this LocalizationSet localizationSet, string parentPath) =>
        localizationSet.Localizations.Select(x => $"{parentPath}/{x.CultureCode}");
}