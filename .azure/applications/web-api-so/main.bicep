targetScope = 'resourceGroup'

@minLength(3)
param imageTag string
@minLength(3)
param environment string
@minLength(3)
param location string
@minLength(3)
param apimUri string

@minLength(3)
@secure()
param containerAppEnvironmentName string
@minLength(3)
@secure()
param appInsightConnectionString string
@minLength(5)
@secure()
param appConfigurationName string
@minLength(3)
@secure()
param environmentKeyVaultName string

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: appConfigurationName
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: containerAppEnvironmentName
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
    value: appConfiguration.properties.endpoint
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

// Only update the base uri for the so version of web api. This feels a bit hackish.
// This needs to be reworked as only this revision will get the updated base uri..
module appConfigConfigurations '../../modules/appConfiguration/upsertKeyValue.bicep' = {
  name: 'AppConfig_Add_DialogPortenBaseUri'
  params: {
    configStoreName: appConfigurationName
    key: 'Application:Dialogporten:BaseUri'
    value: apimUri
    keyValueType: 'custom'
  }
}

module containerApp '../../modules/containerApp/main.bicep' = {
  name: containerAppName
  params: {
    name: containerAppName
    image: '${baseImageUrl}webapi:${imageTag}'
    location: location
    envVariables: containerAppEnvVars
    containerAppEnvId: containerAppEnvironment.id
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
