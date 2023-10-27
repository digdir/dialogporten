using System;

namespace Digdir.Tool.Dialogporten.SlackNotifier;

public class AzureAlert
{
    public string SchemaId { get; set; }
    public Data Data { get; set; }
}

public class Data
{
    public Essentials Essentials { get; set; }
    public Alertcontext AlertContext { get; set; }
    public Customproperties CustomProperties { get; set; }
}

public class Essentials
{
    public string AlertId { get; set; }
    public string AlertRule { get; set; }
    public string Severity { get; set; }
    public string SignalType { get; set; }
    public string MonitorCondition { get; set; }
    public string MonitoringService { get; set; }
    public string[] AlertTargetIDs { get; set; }
    public string[] ConfigurationItems { get; set; }
    public string OriginAlertId { get; set; }
    public DateTime FiredDateTime { get; set; }
    public string Description { get; set; }
    public string EssentialsVersion { get; set; }
    public string AlertContextVersion { get; set; }
}

public class Alertcontext
{
    public Properties Properties { get; set; }
    public string ConditionType { get; set; }
    public Condition Condition { get; set; }
}

public class Properties
{
    public string CanISendProblemId { get; set; }
    public string SomeStaticPayload { get; set; }
}

public class Condition
{
    public string WindowSize { get; set; }
    public Allof[] AllOf { get; set; }
    public DateTime WindowStartTime { get; set; }
    public DateTime WindowEndTime { get; set; }
}

public class Allof
{
    public string SearchQuery { get; set; }
    public object MetricMeasureColumn { get; set; }
    public string TargetResourceTypes { get; set; }
    public string Operator { get; set; }
    public string Threshold { get; set; }
    public string TimeAggregation { get; set; }
    public object[] Dimensions { get; set; }
    public int MetricValue { get; set; }
    public Failingperiods FailingPeriods { get; set; }
    public string LinkToSearchResultsUI { get; set; }
    public string LinkToFilteredSearchResultsUI { get; set; }
    public string LinkToSearchResultsAPI { get; set; }
    public string LinkToFilteredSearchResultsAPI { get; set; }
}

public class Failingperiods
{
    public int NumberOfEvaluationPeriods { get; set; }
    public int MinFailingPeriodsToAlert { get; set; }
}

public class Customproperties
{
    public string CanISendProblemId { get; set; }
    public string SomeStaticPayload { get; set; }
}