import { uniqueStringBySubscriptionAndResourceGroup, uniqueResourceName } from '../../functions/resourceName.bicep'

@description('The location where the resources will be deployed')
param location string

@description('The name of the Application Insights resource')
param applicationInsightsName string

@description('The prefix used for naming resources to ensure unique names')
param namePrefix string

@description('The name of the Key Vault')
param keyVaultName string

@description('Tags to apply to resources')
param tags object

@export()
type Sku = {
  storageAccountName:
    | 'Standard_LRS'
    | 'Standard_GRS'
    | 'Standard_RAGRS'
    | 'Standard_ZRS'
    | 'Premium_LRS'
    | 'Premium_ZRS'
  applicationServicePlanName:
    | 'F1'
    | 'D1'
    | 'B1'
    | 'B2'
    | 'B3'
    | 'S1'
    | 'S2'
    | 'S3'
    | 'P1'
    | 'P2'
    | 'P3'
    | 'P1V2'
    | 'P2V2'
    | 'P3V2'
    | 'I1'
    | 'I2'
    | 'I3'
    | 'Y1'
    | 'Y2'
    | 'Y3'
    | 'Y1v2'
    | 'Y2v2'
    | 'Y3v2'
    | 'Y1v2Isolated'
    | 'Y2v2Isolated'
    | 'Y3v2Isolated'
  applicationServicePlanTier: 'Free' | 'Shared' | 'Basic' | 'Dynamic' | 'Standard' | 'Premium' | 'Isolated'
}
@description('The SKU of the Slack Notifier')
param sku Sku

// Storage account names only supports lower case and numbers
// todo: add name of function as param and turn this into a reusable module
// We use uniqueStringBySubscriptionAndResourceGroup directly here to avoid having too short storage account name.
// This should be refactored to use one common storage account. Or one storage account for all app functions.
var storageAccountName = take(
  replace('${'${namePrefix}-sn'}-${uniqueStringBySubscriptionAndResourceGroup()}', '-', ''),
  24
)

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-04-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: sku.storageAccountName
  }
  kind: 'Storage'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
    minimumTlsVersion: 'TLS1_2'
  }
  tags: tags
}

resource applicationServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: '${namePrefix}-slacknotifier-asp'
  location: location
  sku: {
    name: sku.applicationServicePlanName
    tier: sku.applicationServicePlanTier
  }
  properties: {}
  tags: tags
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

var functionAppNameMaxLength = 40
var functionAppName = uniqueResourceName('${namePrefix}-slacknotifier-fa', functionAppNameMaxLength)
resource functionApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: applicationServicePlan.id
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      // Setting/updating appSettings in separate module in order to not delete already deployed functions, see below
    }
    httpsOnly: true
  }
  tags: tags
}

var appSettings = {
  AzureWebJobsStorage: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
  WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
  WEBSITE_CONTENTSHARE: toLower(functionAppName)
  FUNCTIONS_EXTENSION_VERSION: '~4'
  APPINSIGHTS_INSTRUMENTATIONKEY: applicationInsights.properties.InstrumentationKey
  Slack__WebhookUrl: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Slack--Webhook--Url)'
  FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
}

module updateAppSettings 'appSettings.bicep' = {
  name: '${functionAppName}-appsettings'
  params: {
    webAppName: functionAppName
    currentAppSettings: list(resourceId('Microsoft.Web/sites/config', functionAppName, 'appsettings'), '2023-01-01').properties
    appSettings: appSettings
  }
}

var defaultFunctionKey = listkeys('${functionApp.id}/host/default', '2023-01-01').functionKeys.default
var forwardAlertToSlackTriggerUrl = 'https://${functionApp.properties.defaultHostName}/api/forwardalerttoslack?code=${defaultFunctionKey}'
resource notifyDevTeam 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${namePrefix}-notify-devteam-ag'
  location: 'Global'
  properties: {
    enabled: true
    groupShortName: 'DevNotify'
    azureFunctionReceivers: [
      {
        name: functionApp.properties.defaultHostName
        functionName: 'ForwardAlertToSlack'
        functionAppResourceId: functionApp.id
        httpTriggerUrl: forwardAlertToSlackTriggerUrl
        useCommonAlertSchema: true
      }
    ]
  }
  tags: tags
}

var query = '''
            union
                 (exceptions
                 | where not(customDimensions.['Service Type'] == 'API Management')
                 | where type != "ZiggyCreatures.Caching.Fusion.SyntheticTimeoutException"),
                 (traces
                 | where severityLevel >= 3 or (severityLevel >= 2 and customDimensions.SourceContext startswith "Digdir"))
                 | where customDimensions.RequestPath !startswith "/health"
             | summarize Count = count()
                 by
                 Environment = tostring(customDimensions.EnvironmentName),
                 Type = itemType,
                 problemId,
                 tostring(customDimensions.MessageTemplate),
                 severityLevel
             | extend SeverityLevel = case(
                 severityLevel == 0, "Verbose",
                 severityLevel == 1, "Information",
                 severityLevel == 2, "Warning",
                 severityLevel == 3, "Error",
                 severityLevel == 4, "Critical",
                 tostring(severityLevel)
                                      )
             | extend Details = coalesce(problemId, customDimensions_MessageTemplate)
             | extend Details = replace_string(Details, "Digdir.Domain.Dialogporten.", "")
             | project Environment, Type, SeverityLevel, Count, Details
             '''
resource exceptionOccuredAlertRule 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: '${namePrefix}-exception-occured-sqr'
  location: location
  properties: {
    enabled: true
    severity: 1
    evaluationFrequency: 'PT5M'
    windowSize: 'PT5M'
    scopes: [applicationInsights.id]
    autoMitigate: false
    targetResourceTypes: [
      'microsoft.insights/components'
    ]
    criteria: {
      allOf: [
        {
          query: query
          operator: 'GreaterThan'
          threshold: 0
          timeAggregation: 'Count'
          failingPeriods: {
            numberOfEvaluationPeriods: 1
            minFailingPeriodsToAlert: 1
          }
        }
      ]
    }
    actions: {
      actionGroups: [
        notifyDevTeam.id
      ]
    }
  }
  tags: tags
}

output functionAppPrincipalId string = functionApp.identity.principalId
