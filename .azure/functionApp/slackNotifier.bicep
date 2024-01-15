param location string
param applicationInsightsName string
param namePrefix string
param keyVaultName string

// Storage account names only supports lower case and numbers
var storageAccountName = '${replace(namePrefix, '-', '')}slacknotifiersa'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'Storage'
  properties: {
    supportsHttpsTrafficOnly: true
    defaultToOAuthAuthentication: true
  }
}

resource applicationServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${namePrefix}-slacknotifier-asp'
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
}

var functionAppName = '${namePrefix}-slacknotifier-fa'
resource functionApp 'Microsoft.Web/sites@2023-01-01' = {
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
      // Setting/updating appSettings in separate module in order no not delete already deployed functions, see below
    }
    httpsOnly: true
  }
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
        httpTriggerUrl: 'https://${functionApp.properties.defaultHostName}/api/forewardalerttoslack?code=${defaultFunctionKey}'
        useCommonAlertSchema: true
      }
    ]
  }
}

resource exceptionOccuredAlertRule 'Microsoft.Insights/scheduledQueryRules@2023-03-15-preview' = {
  name: '${namePrefix}-exception-occured-sqr'
  location: location
  properties: {
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
          query: 'exceptions | summarize count = count() by environment = tostring(customDimensions.AspNetCoreEnvironment), problemId'
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
}

output functionAppPrincipalId string = functionApp.identity.principalId
