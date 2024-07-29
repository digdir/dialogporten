using System.Collections.ObjectModel;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Actions;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Activities;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Contents;
using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Domain.Common.Extensions;

public static class AggregateNodeExtensions
{
    public static IReadOnlyCollection<string> ToPaths(this IEnumerable<AggregateNode> aggregateNodes) =>
        ToPaths(aggregateNodes, string.Empty);

    private static ReadOnlyCollection<string> ToPaths(this IEnumerable<AggregateNode> aggregateNodes, string parentPath)
    {
        var paths = new List<string>();

        foreach (var node in aggregateNodes)
        {
            if (node.Entity is LocalizationSet localizationSet)
            {
                paths.AddRange(localizationSet.ToLocalizationPathStrings(
                    $"{parentPath}/{node.Entity.ToName()}"));
                continue;
            }

            if (node.Entity is not IIdentifiableEntity identifiable)
            {
                continue;
            }

            var currentPath = $"{parentPath}/{node.Entity.ToName()}/{identifiable.Id}";

            // Generate modified properties and children paths
            var currentPaths = node.ModifiedProperties
                .Select(x => $"{currentPath}/{x.PropertyName.OverrideName()}")
                .Concat(node.Children.ToPaths(currentPath))
                .DefaultIfEmpty(currentPath);

            paths.AddRange(currentPaths);
        }

        return paths.AsReadOnly();
    }

    private static IEnumerable<string> ToLocalizationPathStrings(this LocalizationSet localizationSet, string parentPath) =>
        localizationSet.Localizations.Select(x => $"{parentPath}/{x.LanguageCode}");

    private static string ToName(this object obj) => obj switch
    {
        DialogEntity => "dialog",
        DialogAttachment => "attachment",
        AttachmentUrl => "url",
        DialogApiAction => "apiAction",
        DialogApiActionEndpoint => "endpoint",
        DialogGuiAction => "guiAction",
        DialogActivity => "activity",
        // ContentValue => "contentValue",
        // Content => "content",
        DialogActivityDescription => "description",
        AttachmentDisplayName => "displayName",
        DialogGuiActionTitle => "title",

        _ => obj.GetType().Name
    };

    private static string OverrideName(this string propertyName) => propertyName switch
    {
        string name when name.EndsWith("Id", StringComparison.InvariantCultureIgnoreCase) => name[..^2],
        _ => propertyName
    };
}
