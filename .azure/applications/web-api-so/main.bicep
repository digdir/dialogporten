targetScope = 'resourceGroup'

param imageTag string
param environment string
param location string

param containerAppEnvironmentId string
param appInsightConnectionString string
param appConfigurationName string

@secure()
param environmentKeyVaultName string

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: '${namePrefix}-appConfiguration'
}

var containerAppEnvVars = [
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: environment
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightConnectionString
  }
  {
    name: 'AZURE_APPCONFIG_URI'
    value: appConfig.properties.endpoint
  }
  {
    name: 'ASPNETCORE_URLS'
    value: 'http://+:8080'
  }
  {
    name: 'RUN_OUTBOX_SCHEDULER'
    value: 'true'
  }
]

resource environmentKeyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: environmentKeyVaultName
}

var containerAppName = '${namePrefix}-webapi-so-ca'

module containerApp '../../modules/containerApp/main.bicep' = {
  name: containerAppName
  params: {
    name: containerAppName
    image: '${baseImageUrl}webapi:${imageTag}'
    location: location
    envVariables: containerAppEnvVars
    containerAppEnvId: containerAppEnvironmentId
  }
}

module keyVaultReaderAccessPolicy '../../modules/keyvault/addReaderRoles.bicep' = {
  name: 'keyVaultReaderAccessPolicy-${containerAppName}'
  params: {
    keyvaultName: environmentKeyVaultResource.name
    principalIds: [ containerApp.outputs.identityPrincipalId ]
  }
}

module appConfigReaderAccessPolicy '../../modules/appConfiguration/addReaderRoles.bicep' = {
  name: 'appConfigReaderAccessPolicy-${containerAppName}'
  params: {
    appConfigurationName: appConfigurationName
    principalIds: [ containerApp.outputs.identityPrincipalId ]
  }
}

output name string = containerApp.outputs.name
output revisionName string = containerApp.outputs.revisionName
