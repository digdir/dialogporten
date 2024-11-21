using Digdir.Tool.Dialogporten.SlackNotifier.External.AppInsights;
using Digdir.Tool.Dialogporten.SlackNotifier.Features.AzureAlertToSlackForwarder;

namespace Digdir.Tool.Dialogporten.SlackNotifier.Common;

internal static class Extensions
{
    public static string ToAsciiTableExceptionReport(this IEnumerable<AppInsightsQueryResponseDto> responses)
    {
        var asciiTables = responses.SelectMany(x => x.Tables)
            .Select(table => Enumerable.Empty<List<object>>()
                .Append(table.Columns.Select(x => (object)x.Name))
                .Concat(table.Rows)
                .ToAsciiTable());
        return string.Join(Environment.NewLine, asciiTables);
    }

    public static string ToQueryLink(this AzureAlertDto azureAlertRequest)
    {
        // This is a predefined KQL query that will get all exceptions for the last 24h, ordered by timestamp descending.
        const string encodedKqlQuery = "H4sIAAAAAAAAA0utSE4tKMnMzyvmqlHIL0pJLVJIqlQoycxNLS5JzC1QSEktTgYAbgDhFSQAAAA%253D/timespan/P1D";
        var link = azureAlertRequest.Data.AlertContext.Condition.AllOf
            .Select(x => x.LinkToFilteredSearchResultsUI)
            .First(x => !string.IsNullOrWhiteSpace(x));
        link = RemoveQuery(link) + encodedKqlQuery;
        return link;
    }

    private static string RemoveQuery(string inputUrl)
    {
        var index = inputUrl.IndexOf("q/", StringComparison.Ordinal);

        if (index >= 0)
        {
            return inputUrl[..(index + 2)]; // Include "q/"
        }

        return inputUrl; // "q/" not found, return the original URL
    }
}
