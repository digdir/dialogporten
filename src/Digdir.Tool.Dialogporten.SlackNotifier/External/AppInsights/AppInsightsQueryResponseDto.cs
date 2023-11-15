namespace Digdir.Tool.Dialogporten.SlackNotifier.External.AppInsights;

internal sealed class AppInsightsQueryResponseDto
{
    public required Table[] Tables { get; set; }
}

internal sealed class Table
{
    public required Column[] Columns { get; set; }
    public required List<List<object>> Rows { get; set; }
}

internal sealed class Column
{
    public required string Name { get; set; }
}
