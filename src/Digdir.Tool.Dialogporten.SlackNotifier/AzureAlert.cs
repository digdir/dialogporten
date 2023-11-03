namespace Digdir.Tool.Dialogporten.SlackNotifier;

public class AzureAlert
{
    public Data Data { get; set; }
}

public class Data
{
    public Alertcontext AlertContext { get; set; }
}

public class Alertcontext
{
    public Condition Condition { get; set; }
}

public class Condition
{
    public Allof[] AllOf { get; set; }
}

public class Allof
{
    public string LinkToFilteredSearchResultsUI { get; set; }
    public string LinkToFilteredSearchResultsAPI { get; set; }
}