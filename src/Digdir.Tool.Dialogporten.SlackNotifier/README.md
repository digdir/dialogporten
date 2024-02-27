# Slack notifier
This function app is designed to convert Azure log alert v2 to a Slack message formatted as an ASCII table. 

When a Azure log alert triggers it notifies every consumer of the configured Azure action group. One of the consumers is this Azure function app which receives a HTTP Post request with the [log alert v2 format](https://learn.microsoft.com/en-us/azure/azure-monitor/alerts/alerts-common-schema#sample-log-alert-when-the-monitoringservice--log-alerts-v2). See [AzureAlertDto.cs](./Features/AzureAlertToSlackForwarder/AzureAlertDto.cs) for the format this function app expects. 

The log alert v2 format does not include the actual query data which triggered the alert. Therefore the function app must fetch it by calling application insight. The data is then transformed to an ASCII table and pushed to the configured Slack webhook URL through the field `exceptionReport`. It will also include a link to the application insight log with the following predefined query in the field named `link`:
```KQL
exceptions
| order by timestamp desc
```

The configured Slack webhook will receive the following request:
```HTTP
HTTP POST [Slack_Webhook_Url]
{
    "exceptionReport": "Ascii_table_as_string",
    "link": "Link_to_application_insight",
}
```

## Local development
1. [Login to azure](https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/?tabs=command-line#exploring-the-sequence-of-defaultazurecredential-authentication-methods)
2. Configure the Slack webhook URL
    ```powerhell
    dotnet user-secrets set -p .\src\Digdir.Tool.Dialogporten.SlackNotifier\ "Slack:WebhookUrl" "SLACK_WEBHOOK_URL_HERE"
    ```
3. Start the function app
4. Send a log alert v2 format to the app

The configured URL doesn't have to be an actual Slack workflow webhook URL. It could point to an online webhook tester like https://webhook.site or a homemade webhook tester on your local machine.

### Get a valid log alert v2 request
This function app uses the links in the incoming alerts request to fetch data. Therefore the requests are app instance and time specific. The provided example request is most likely to be invalid by the time this article is read. Do the following to get a valid request: 
1. Go to https://webhook.site and copy your unique URL
2. Add the URL as a webhook action of the azure action group 
3. Trigger the alert
4. Copy the request from https://webhook.site into Postman. It may take several minutes for the alert to produce a request to the webhook.
5. Delete the webhook action from the azure action group

Example log alert v2 request:
```jsonc
{
  "schemaId": "azureMonitorCommonAlertSchema",
  "data": {
    "essentials": {
      "alertId": "/subscriptions/052982ed-1e94-4e26-bfd4-a65252931325/providers/Microsoft.AlertsManagement/alerts/3de19cbd-afe1-1d68-219c-25a339960013",
      "alertRule": "Exception occured",
      "severity": "Sev1",
      "signalType": "Log",
      "monitorCondition": "Fired",
      "monitoringService": "Log Alerts V2",
      "alertTargetIDs": [
        "/subscriptions/052982ed-1e94-4e26-bfd4-a65252931325/resourcegroups/dppoc-rg/providers/microsoft.insights/components/dppoc-applicationinsights"
      ],
      "configurationItems": [
        "/subscriptions/052982ed-1e94-4e26-bfd4-a65252931325/resourceGroups/dppoc-rg/providers/microsoft.insights/components/dppoc-applicationInsights"
      ],
      "originAlertId": "b7bbd427-e2a0-4891-8bcc-c689f7bf30e4",
      "firedDateTime": "2023-11-03T11:48:02.7701858Z",
      "description": "",
      "essentialsVersion": "1.0",
      "alertContextVersion": "1.0"
    },
    "alertContext": {
      "conditionType": "LogQueryCriteria",
      "condition": {
        "windowSize": "PT5M",
        "allOf": [
          {
            "searchQuery": "exceptions\n| summarize count = count() by environment = tostring(customDimensions.AspNetCoreEnvironment), problemId\n\n",
            "metricMeasureColumn": null,
            "targetResourceTypes": "['microsoft.insights/components']",
            "operator": "GreaterThan",
            "threshold": "0",
            "timeAggregation": "Count",
            "dimensions": [],
            "metricValue": 1.0,
            "failingPeriods": {
              "numberOfEvaluationPeriods": 1,
              "minFailingPeriodsToAlert": 1
            },
            "linkToSearchResultsUI": "https://portal.azure.com#@cd0026d8-283b-4a55-9bfa-d0ef4a8ba21c/blade/Microsoft_Azure_Monitoring_Logs/LogsBlade/source/Alerts.EmailLinks/scope/%7B%22resources%22%3A%5B%7B%22resourceId%22%3A%22%2Fsubscriptions%2F052982ed-1e94-4e26-bfd4-a65252931325%2FresourceGroups%2Fdppoc-rg%2Fproviders%2Fmicrosoft.insights%2Fcomponents%2Fdppoc-applicationInsights%22%7D%5D%7D/q/eJxLrUhOLSjJzM8r5qpRKC7NzU0syqxKVUjOL80rUbCF0BqaCkmVCql5ZZlF%2BXm5qWCJkvzikqLMvHSN5NLikvxcl0ygeDHIGD3H4gK%2F1BLn%2FKJUV4QOTR2FgqL8pJzUXM8UAA%3D%3D/prettify/1/timespan/2023-11-03T11%3a42%3a25.0000000Z%2f2023-11-03T11%3a47%3a25.0000000Z",
            "linkToFilteredSearchResultsUI": "https://portal.azure.com#@cd0026d8-283b-4a55-9bfa-d0ef4a8ba21c/blade/Microsoft_Azure_Monitoring_Logs/LogsBlade/source/Alerts.EmailLinks/scope/%7B%22resources%22%3A%5B%7B%22resourceId%22%3A%22%2Fsubscriptions%2F052982ed-1e94-4e26-bfd4-a65252931325%2FresourceGroups%2Fdppoc-rg%2Fproviders%2Fmicrosoft.insights%2Fcomponents%2Fdppoc-applicationInsights%22%7D%5D%7D/q/eJxLrUhOLSjJzM8r5qpRKC7NzU0syqxKVUjOL80rUbCF0BqaCkmVCql5ZZlF%2BXm5qWCJkvzikqLMvHSN5NLikvxcl0ygeDHIGD3H4gK%2F1BLn%2FKJUV4QOTR2FgqL8pJzUXM8UAA%3D%3D/prettify/1/timespan/2023-11-03T11%3a42%3a25.0000000Z%2f2023-11-03T11%3a47%3a25.0000000Z",
            "linkToSearchResultsAPI": "https://api.applicationinsights.io/v1/apps/3a744853-dbe6-4ad8-91ff-c585c79c4ce5/query?query=exceptions%0A%7C%20summarize%20count%20%3D%20count%28%29%20by%20environment%20%3D%20tostring%28customDimensions.AspNetCoreEnvironment%29%2C%20problemId&timespan=2023-11-03T11%3a42%3a25.0000000Z%2f2023-11-03T11%3a47%3a25.0000000Z",
            "linkToFilteredSearchResultsAPI": "https://api.applicationinsights.io/v1/apps/3a744853-dbe6-4ad8-91ff-c585c79c4ce5/query?query=exceptions%0A%7C%20summarize%20count%20%3D%20count%28%29%20by%20environment%20%3D%20tostring%28customDimensions.AspNetCoreEnvironment%29%2C%20problemId&timespan=2023-11-03T11%3a42%3a25.0000000Z%2f2023-11-03T11%3a47%3a25.0000000Z"
          }
        ],
        "windowStartTime": "2023-11-03T11:42:25Z",
        "windowEndTime": "2023-11-03T11:47:25Z"
      }
    },
    "customProperties": {
      "CanISendProblemId": "$problemId",
      "SomeStaticPayload": "staticPayload"
    }
  }
}

```
