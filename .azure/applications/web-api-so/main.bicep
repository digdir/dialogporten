targetScope = 'resourceGroup'

param imageTag string
param gitSha string

param environment string
param location string

// todo: this needs to be output from infrastructure.bicep and overkill with both id and name here lawl
param containerAppEnvironmentId string

// todo: refactor to something else
param appInsightConnectionString string

// appConfiguration.outputs.name
param appConfigurationName string

@secure()
param environmentKeyVaultName string

@secure()
param sourceKeyVaultSubscriptionId string
@secure()
param sourceKeyVaultResourceGroup string
@secure()
param sourceKeyVaultName string

var secrets = {
  sourceKeyVaultSubscriptionId: sourceKeyVaultSubscriptionId
  sourceKeyVaultResourceGroup: sourceKeyVaultResourceGroup
  sourceKeyVaultName: sourceKeyVaultName
}

var namePrefix = 'dp-be-${environment}'
var baseImageUrl = 'ghcr.io/digdir/dialogporten-'

// todo: can we mount the environment variables from app configuration directly?

// todo: add bicepparam file

// todo: solve this some other way pls
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
    name: 'GIT_SHA'
    value: gitSha
  }
]

resource srcKeyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: secrets.sourceKeyVaultName
  scope: az.resourceGroup(secrets.sourceKeyVaultSubscriptionId, secrets.sourceKeyVaultResourceGroup)
}

resource environmentKeyVaultResource 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: environmentKeyVaultName
  scope: az.resourceGroup(secrets.sourceKeyVaultSubscriptionId, secrets.sourceKeyVaultResourceGroup)
}

// todo: split the module into two. One for the migration job and one for the app itself

var containerAppName = '${namePrefix}-webapi-so-ca'

module containerApp '../../modules/containerApp/main.bicep' = {
  // todo: change to use the name of the app
  name: containerAppName
  params: {
    name: containerAppName
    image: '${baseImageUrl}webapi:${imageTag}'
    initContainerimage: '${baseImageUrl}migration-verifier:${imageTag}'
    location: location
    envVariables: containerAppEnvVars
    migrationVerifierPrincipalAppId: srcKeyVaultResource.getSecret('MigrationVerificationInitContainerPrincipalAppId')
    migrationVerifierPrincipalPassword: srcKeyVaultResource.getSecret('MigrationVerificationInitContainerPrincipalPassword')
    containerAppEnvId: containerAppEnvironmentId
    // todo: get from input
    migrationJobName: '${namePrefix}-migration-job'
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
