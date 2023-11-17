namespace Digdir.Tool.Dialogporten.SlackNotifier.Features.AzureAlertToSlackForwarder;

public class AzureAlertDto
{
    public required AzureAlertDataDto Data { get; set; }
}

public class AzureAlertDataDto
{
    public required AzureAlertContextDto AlertContext { get; set; }
}

public class AzureAlertContextDto
{
    public required AzureAlertConditionDto Condition { get; set; }
}

public class AzureAlertConditionDto
{
    public required AzureAlertAllofDto[] AllOf { get; set; }
}

public class AzureAlertAllofDto
{
    public required string LinkToFilteredSearchResultsUI { get; set; }
    public required string LinkToFilteredSearchResultsAPI { get; set; }
}