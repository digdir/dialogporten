using System.Collections.Generic;

namespace Digdir.Tool.Dialogporten.SlackNotifier;

public class AppInsightsQueryResponse
{
    public Table[] Tables { get; set; }
}

public class Table
{
    public Column[] Columns { get; set; }
    public List<List<object>> Rows { get; set; }
}

public class Column
{
    public string Name { get; set; }
}
