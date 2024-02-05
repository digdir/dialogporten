targetScope = 'resourceGroup'

param imageTag string
param environment string
param location string

param containerAppEnvironmentId string
// todo: refactor to something else
param appInsightConnectionString string
param appConfigurationName string

@secure()
param environmentKeyVaultName string

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

// todo: solve this some other way pls
resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' existing = {
  name: '${namePrefix}-appConfiguration'
}

// todo: can we mount the environment variables from app configuration directly?
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
]

resource environmentKeyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: environmentKeyVaultName
}

var containerAppName = '${namePrefix}-webapi-eu-ca'

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
